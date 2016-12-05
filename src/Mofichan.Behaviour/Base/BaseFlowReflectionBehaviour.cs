using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Mofichan.Behaviour.Flow;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.Behaviour.Base
{
    /// <summary>
    /// A derived version of <see cref="BaseFlowBehaviour"/> that allows for
    /// flows to be partially defined through reflection.
    /// <para></para>
    /// The intention of this class is to allow for more comprehensible behaviour
    /// modules that defined behavioural flows.
    /// </summary>
    public abstract class BaseFlowReflectionBehaviour : BaseFlowBehaviour
    {
        private const BindingFlags CandidateBindingFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        private readonly Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> transitionManagerFactory;
        private readonly ISet<IFlowNode> nodes;
        private readonly ISet<IFlowTransition> transitions;
        private readonly ISet<Tuple<string, string, string>> connections;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseFlowReflectionBehaviour" /> class.
        /// </summary>
        /// <param name="startNodeId">Identifies the starting node in the flow.</param>
        /// <param name="responseBuilderFactory">The response builder factory.</param>
        /// <param name="flowManager">The flow manager.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="passThroughMessages">If set to <c>true</c>, passes through unhandled messages.</param>
        protected BaseFlowReflectionBehaviour(
            string startNodeId,
            Func<IResponseBuilder> responseBuilderFactory,
            IFlowManager flowManager,
            ILogger logger,
            bool passThroughMessages = true)
            : base(startNodeId, responseBuilderFactory, flowManager, logger, passThroughMessages)
        {
            this.transitionManagerFactory = flowManager.BuildTransitionManager;
            this.nodes = new HashSet<IFlowNode>();
            this.transitions = new HashSet<IFlowTransition>();
            this.connections = new HashSet<Tuple<string, string, string>>();
        }

        #region Common Nodes
        /// <summary>
        /// Registers a commonly-used node that will transition using the <see cref="IFlowTransition"/>
        /// specified by <paramref name="successTransitionId"/> if and only if the user the message
        /// is from has Mofichan's attention.
        /// <para></para>
        /// Otherwise, the transition corresponding to <paramref name="failureTransitionId"/> will be
        /// used instead.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="successTransitionId">The success transition identifier.</param>
        /// <param name="failureTransitionId">The failure transition identifier.</param>
        protected void RegisterAttentionGuardNode(
            string nodeId,
            string successTransitionId,
            string failureTransitionId)
        {
            var node = this.CreateNode(nodeId, (context, manager) =>
            {
                var tags = context.Message.Tags;
                var user = context.Message.From as IUser;

                if (user == null)
                {
                    manager.MakeTransitionCertain(failureTransitionId);
                }

                bool isPayingAttentionToUser = context.Attention.IsPayingAttentionToUser(user);

                if (tags.Contains("directedAtMofichan") || isPayingAttentionToUser)
                {
                    manager.MakeTransitionCertain(successTransitionId);
                }
                else
                {
                    manager.MakeTransitionCertain(failureTransitionId);
                }
            });

            this.nodes.Add(node);
        }

        /// <summary>
        /// Registers the identifier of a simple <see cref="IFlowNode"/> that has no associated
        /// action and no special properties.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        protected void RegisterSimpleNode(string nodeId)
        {
            var node = this.CreateNode(nodeId, (c, m) => { });
            this.nodes.Add(node);
        }

        #endregion Common Nodes

        #region Common Transitions
        /// <summary>
        /// Registers and connects a transition that will loop back to itself
        /// while the user the message is from has Mofichan's attention.
        /// </summary>
        /// <param name="transitionId">The transition identifier.</param>
        /// <param name="onAttentionLossTransitionId">
        /// The ID of the transition to take when the user loses Mofichan's attention.
        /// </param>
        /// <param name="from">The identifier of the node to transition from.</param>
        /// <param name="to">The identifier of the node to transition to.</param>
        protected void RegisterAttentionGuardTransition(
            string transitionId,
            string onAttentionLossTransitionId,
            string from,
            string to)
        {
            var transition = new FlowTransition(transitionId, (context, manager) =>
            {
                var user = context.Message.From as IUser;
                Debug.Assert(user != null, "The message should be from a user");

                if (!context.Attention.IsPayingAttentionToUser(user))
                {
                    manager.ClearTransitionWeights();
                    manager.MakeTransitionCertain(onAttentionLossTransitionId);
                }
            });

            this.transitions.Add(transition);
            this.connections.Add(Tuple.Create(from, to, transitionId));
        }

        /// <summary>
        /// Registers the identifier of a simple <see cref="IFlowTransition"/> that has no
        /// associated action and no special properties.
        /// </summary>
        /// <param name="transitionId">The transition identifier.</param>
        /// <param name="from">The identifier of the node to transition from.</param>
        /// <param name="to">The identifier of the node to transition to.</param>
        protected void RegisterSimpleTransition(string transitionId, string from, string to)
        {
            this.transitions.Add(new FlowTransition(transitionId));
            this.connections.Add(Tuple.Create(from, to, transitionId));
        }
        #endregion

        /// <summary>
        /// Configures the <see cref="IFlow"/> for this instance based on both the declared
        /// methods within the concrete type and additional nodes or transitions declared
        /// through <see cref="RegisterSimpleNode(string)"/> and
        /// <see cref="RegisterSimpleTransition(string, string, string)"/> respectively.
        /// </summary>
        protected void Configure()
        {
            var candidateMethods = this.GetType().GetTypeInfo().GetMethods(CandidateBindingFlags);

            var declaredNodes = from methodInfo in candidateMethods
                                where HasValidSignature(methodInfo)
                                let stateAttr = methodInfo.GetCustomAttribute<FlowStateAttribute>()
                                where stateAttr != null
                                let stateId = stateAttr.Id
                                let distinctUntilChanged = stateAttr.DistinctUntilChanged
                                let stateAction = this.CreateStateAction(methodInfo, distinctUntilChanged)
                                select new FlowNode(stateId, stateAction, this.transitionManagerFactory);

            var declaredConnections = from methodInfo in candidateMethods
                                      where HasValidSignature(methodInfo)
                                      from transAttr in methodInfo.GetCustomAttributes<FlowTransitionAttribute>()
                                      let transId = transAttr.Id
                                      let transAction = this.CreateTransitionAction(methodInfo)
                                      let transition = new FlowTransition(transId, transAction)
                                      let nodeFrom = transAttr.From
                                      let nodeTo = transAttr.To
                                      select new { nodeFrom, nodeTo, transition };

            var declaredTransitions = declaredConnections.Select(it => it.transition);

            bool multipleNodeDefinitions = this.nodes.Intersect(declaredNodes).Any();
            bool multipleTransitionDefinitions = this.transitions.Intersect(declaredTransitions).Any();

            Raise.InvalidOperationException.If(multipleNodeDefinitions,
                "One or more nodes have multiple definitions");

            Raise.InvalidOperationException.If(multipleTransitionDefinitions,
                "One or more transitions have multiple definitions");

            this.nodes.UnionWith(declaredNodes);
            this.transitions.UnionWith(declaredTransitions);
            this.connections.UnionWith(declaredConnections.Select(
                it => Tuple.Create(it.nodeFrom, it.nodeTo, it.transition.Id)));

            this.RegisterFlow(flowBuilder =>
            {
                flowBuilder.WithNodes(this.nodes).WithTransitions(this.transitions);

                this.connections.ToList().ForEach(it => flowBuilder.WithConnection(it.Item1, it.Item2, it.Item3));

                return flowBuilder;
            });
        }

        private IFlowNode CreateNode(string nodeId, Action<FlowContext, IFlowTransitionManager> nodeAction)
        {
            return new FlowNode(nodeId, nodeAction, this.transitionManagerFactory);
        }

        private static bool HasValidSignature(MethodInfo methodInfo)
        {
            Func<bool> hasTwoParameters =
                () => methodInfo.GetParameters().Length == 2;

            Func<bool> firstParamIsFlowContext =
                () => methodInfo.GetParameters()[0].ParameterType == typeof(FlowContext);

            Func<bool> secondParamIsFlowTransitionManager =
                () => methodInfo.GetParameters()[1].ParameterType == typeof(IFlowTransitionManager);

            return hasTwoParameters()
                && firstParamIsFlowContext()
                && secondParamIsFlowTransitionManager();
        }

        private Action<FlowContext, IFlowTransitionManager> CreateStateAction(MethodInfo methodInfo, bool distinctUntilChanged)
        {
            Debug.Assert(HasValidSignature(methodInfo), "The method should have a valid signature");

            var distinctContexts = new HashSet<FlowContext>();

            return (c, m) =>
            {
                if (distinctUntilChanged && !distinctContexts.Add(c))
                {
                    return;
                }
                else if (distinctUntilChanged)
                {
                    distinctContexts.Clear();
                    bool added = distinctContexts.Add(c);

                    Debug.Assert(added, "The first element of the flow context set should always be added");
                }

                try
                {
                    methodInfo.Invoke(this, new object[] { c, m });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            };
        }

        private Action<FlowContext, IFlowTransitionManager> CreateTransitionAction(MethodInfo methodInfo)
        {
            Debug.Assert(HasValidSignature(methodInfo), "The method should have a valid signature");

            return (c, m) =>
            {
                try
                {
                    methodInfo.Invoke(this, new object[] { c, m });
                }
                catch (TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            };
        }
    }
}
