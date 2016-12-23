using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.BotState;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using NodeAction = System.Func<
    Mofichan.Core.Flow.FlowContext,
    Mofichan.Core.Flow.FlowTransitionManager,
    Mofichan.Core.Visitor.IBehaviourVisitor,
    Mofichan.Core.Flow.FlowNodeState?>;

namespace Mofichan.Tests.TestUtility
{
    public static class FlowUtil
    {
        public static NodeAction NullStateAction
        {
            get
            {
                return (c, m, v) => null;
            }
        }

        public static Func<IPulseDriver, IAttentionManager> AttentionManagerFactory
        {
            get
            {
                return driver => new PulseDrivenAttentionManager(1000, 20, driver, MockLogger.Instance);
            }
        }

        public static BaseFlow.Builder<T> ConfigureDefaultFlowBuilder<T>(
            BaseFlow.Builder<T> flowBuilder,
            IEnumerable<IFlowNode> nodes,
            IEnumerable<IFlowTransition> transitions)
            where T : BaseFlow
        {
            return flowBuilder
                .WithStartNodeId(nodes.First().Id)
                .WithNodes(nodes)
                .WithTransitions(transitions);
        }

        public static NodeAction DecideTransitionFromMatch(
            string toMatch, string successTransitionId)
        {
            return (context, manager, visitor) =>
            {
                var body = context.Message.Body;

                if (body.Contains(toMatch))
                {
                    manager.MakeTransitionCertain(successTransitionId);
                }
                else
                {
                    manager.MakeTransitionsImpossible();
                }

                return null;
            };
        }
    }
}
