using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Serilog;

namespace Mofichan.Tests.TestUtility
{
    public static class FlowUtil
    {
        public static Action<FlowContext, IFlowTransitionManager> NullStateAction
        {
            get
            {
                return (c, m) => { };
            }
        }

        public static Func<IEnumerable<IFlowTransition>, IFlowTransitionManager> TransitionManagerFactory
        {
            get
            {
                return transitions => new FlowTransitionManager(transitions);
            }
        }

        public static Func<IPulseDriver, IAttentionManager> AttentionManagerFactory
        {
            get
            {
                return driver => new PulseDrivenAttentionManager(1000, 20, driver, MockLogger.Instance);
            }
        }

        public static BasicFlow.Builder CreateDefaultFlowBuilder(
            IEnumerable<IFlowNode> nodes,
            IEnumerable<IFlowTransition> transitions)
        {
            var manager = new FlowManager(null);

            return new BasicFlow.Builder()
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithManager(manager)
                .WithStartNodeId(nodes.First().Id)
                .WithNodes(nodes)
                .WithTransitions(transitions);
        }

        public static Action<FlowContext, IFlowTransitionManager> DecideTransitionFromMatch(
            string toMatch, string successTransitionId)
        {
            return (context, manager) =>
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
            };
        }
    }
}
