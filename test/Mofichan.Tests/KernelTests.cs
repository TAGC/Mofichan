using System;
using System.Collections.Generic;
using System.Linq;
using Mofichan.Core;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using Mofichan.Core.Visitor;
using Mofichan.Tests.TestUtility;
using Moq;
using Xunit;

namespace Mofichan.Tests
{
    public class KernelTests
    {
        [Fact]
        public void Kernel_Should_Throw_Exception_If_Null_Behaviour_Collection_Provided()
        {
            var backend = Mock.Of<IMofichanBackend>();
            var chainBuilder = Mock.Of<IBehaviourChainBuilder>();
            var pulseDriver = Mock.Of<IPulseDriver>();
            Func<IMessageClassifier> messageClassifierFactory = Mock.Of<IMessageClassifier>;
            var visitorFactory = Mock.Of<IBehaviourVisitorFactory>();
            var responseSelector = Mock.Of<IResponseSelector>();
            var logger = MockLogger.Instance;

            // GIVEN an null behaviour collection.
            IEnumerable<IMofichanBehaviour> behaviours = null;

            // EXPECT an exception to be thrown if we construct a kernel with it.
            Assert.Throws<ArgumentNullException>(() =>
                new Kernel(backend, behaviours, chainBuilder, pulseDriver, messageClassifierFactory, visitorFactory,
                    responseSelector, logger));
        }

        [Fact]
        public void Kernel_Should_Throw_Exception_If_No_Behaviours_Provided()
        {
            var backend = Mock.Of<IMofichanBackend>();
            var chainBuilder = Mock.Of<IBehaviourChainBuilder>();
            var pulseDriver = Mock.Of<IPulseDriver>();
            Func<IMessageClassifier> messageClassifierFactory = Mock.Of<IMessageClassifier>;
            var visitorFactory = Mock.Of<IBehaviourVisitorFactory>();
            var responseSelector = Mock.Of<IResponseSelector>();
            var logger = MockLogger.Instance;

            // GIVEN an empty behaviour collection.
            IEnumerable<IMofichanBehaviour> behaviours = Enumerable.Empty<IMofichanBehaviour>();

            // EXPECT an exception to be thrown if we construct a kernel with it.
            Assert.Throws<ArgumentException>(() =>
                new Kernel(backend, behaviours, chainBuilder, pulseDriver, messageClassifierFactory, visitorFactory,
                    responseSelector, logger));
        }

        [Fact]
        public void Kernel_Should_Start_Backend_When_Started()
        {
            var behaviours = new[] { Mock.Of<IMofichanBehaviour>() };
            var chainBuilder = new BehaviourChainBuilder();
            var pulseDriver = Mock.Of<IPulseDriver>();
            Func<IMessageClassifier> messageClassifierFactory = Mock.Of<IMessageClassifier>;
            var visitorFactory = Mock.Of<IBehaviourVisitorFactory>();
            var responseSelector = Mock.Of<IResponseSelector>();
            var logger = MockLogger.Instance;

            // GIVEN a mock backend.
            var backend = new Mock<IMofichanBackend>();

            // GIVEN a kernel constructed with the backend.
            var kernel = new Kernel(backend.Object, behaviours, chainBuilder, pulseDriver, messageClassifierFactory,
                visitorFactory, responseSelector, logger);

            // WHEN we start the kernel.
            kernel.Start();

            // THEN the backend should have been started.
            backend.Verify(it => it.Start(), Times.Once);
        }
    }
}
