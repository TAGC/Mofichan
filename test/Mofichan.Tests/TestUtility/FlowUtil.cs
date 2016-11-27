﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Behaviour.Flow;
using Mofichan.Core;
using Mofichan.Core.Flow;
using Mofichan.Core.Interfaces;
using Moq;
using Serilog;

namespace Mofichan.Tests.TestUtility
{
    public static class FlowUtil
    {
        public static Action<OutgoingMessage> DiscardResponse
        {
            get
            {
                return _ => { };
            }
        }

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

        public static BasicFlow.Builder CreateDefaultFlowBuilder(
            IEnumerable<IFlowNode> nodes,
            IEnumerable<IFlowTransition> transitions)
        {
            return new BasicFlow.Builder()
                .WithLogger(new LoggerConfiguration().CreateLogger())
                .WithDriver(Mock.Of<IFlowDriver>())
                .WithTransitionSelector(new FairFlowTransitionSelector())
                .WithGeneratedResponseHandler(DiscardResponse)
                .WithStartNodeId(nodes.First().Id)
                .WithNodes(nodes)
                .WithTransitions(transitions);
        }

        public static Action<FlowContext, IFlowTransitionManager> DecideTransitionFromMatch(
            string toMatch, string successTransitionId, string failTransitionId)
        {
            return new Action<FlowContext, IFlowTransitionManager>((context, manager) =>
            {
                var body = context.Message.Body;

                if (body.Contains(toMatch))
                {
                    manager.MakeTransitionCertain(successTransitionId);
                }
                else
                {
                    manager.MakeTransitionCertain(failTransitionId);
                }
            });
        }
    }
}