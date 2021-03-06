﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using Mofichan.Core.BehaviourOutputs;
using Mofichan.Core.Exceptions;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Visitor;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.Core
{
    /// <summary>
    /// Represents Mofichan's core.
    /// <para></para>
    /// Instances of this class act as bridges between the configured <see cref="IMofichanBackend"/>
    /// and Mofichan's configured behaviour chain.
    /// </summary>
    public sealed class Kernel : IDisposable
    {
        private readonly IPulseDriver pulseDriver;
        private readonly Func<IMessageClassifier> messageClassifierFactory;
        private readonly IBehaviourVisitorFactory visitorFactory;
        private readonly IResponseSelector responseSelector;
        private readonly IMofichanBackend backend;
        private readonly IMofichanBehaviour rootBehaviour;
        private readonly Queue<MessageContext> pendingReceivedMessages;
        private readonly ILogger logger;

        private List<IMofichanBehaviour> behaviours;
        private bool responsePending = false;
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="Kernel" /> class.
        /// </summary>
        /// <param name="backend">The selected backend.</param>
        /// <param name="behaviours">The collection of behaviours to determine Mofichan's personality.</param>
        /// <param name="chainBuilder">The object to use to compose the provided behaviours into a chain.</param>
        /// <param name="pulseDriver">The pulse driver.</param>
        /// <param name="messageClassifierFactory">
        /// A factory to produce objects to classify messages Mofichan receives.
        /// </param>
        /// <param name="visitorFactory">A factory to produce instances of behaviour visitors.</param>
        /// <param name="responseSelector">The object to use to choose responses obtained by visitors.</param>
        /// <param name="logger">The logger to use.</param>
        public Kernel(
            IMofichanBackend backend,
            IEnumerable<IMofichanBehaviour> behaviours,
            IBehaviourChainBuilder chainBuilder,
            IPulseDriver pulseDriver,
            Func<IMessageClassifier> messageClassifierFactory,
            IBehaviourVisitorFactory visitorFactory,
            IResponseSelector responseSelector,
            ILogger logger)
        {
            Raise.ArgumentNullException.IfIsNull(behaviours, nameof(behaviours));
            Raise.ArgumentException.IfNot(behaviours.Any(), nameof(behaviours),
                "At least one behaviour must be specified for Mofichan");

            this.pulseDriver = pulseDriver;
            this.messageClassifierFactory = messageClassifierFactory;
            this.visitorFactory = visitorFactory;
            this.responseSelector = responseSelector;
            this.backend = backend;
            this.pendingReceivedMessages = new Queue<MessageContext>();
            this.logger = logger.ForContext<Kernel>();

            this.logger.Information("Initialising Mofichan with {Backend} and {Behaviours}", backend, behaviours);

            this.responseSelector.ResponseSelected += (s, e) => this.OnResponseSelected(e.Response);
            this.responseSelector.ResponseWindowExpired += (s, e) => this.OnResponseWindowExpired(e.RespondingTo);
            this.rootBehaviour = this.BuildBehaviourChain(behaviours, chainBuilder);

            this.PairRootBehaviourToBackend();
        }

        /// <summary>
        /// Starts Mofichan.
        /// <para></para>
        /// This will cause the specified backend and modules in the behaviour chain
        /// to initialise.
        /// </summary>
        public void Start()
        {
            Debug.Assert(this.behaviours != null, "The behaviour list should have been set");

            this.backend.Start();
            this.behaviours.ForEach(it => it.Start());
            this.logger.Information("Initialised Mofichan");
        }

        #region IDisposable Support
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                this.logger.Information("Disposing Mofichan");
                if (disposing)
                {
                    this.backend.Dispose();
                    this.behaviours?.ForEach(it => it.OnCompleted());
                }

                this.disposedValue = true;
            }
        }
        #endregion

        private void PairRootBehaviourToBackend()
        {
            Func<MessageContext, bool> notFromMofi = it => (it.From as IUser)?.Type != UserType.Self;

            this.backend.Where(notFromMofi).Subscribe(message => this.OnReceiveMessage(message));

            this.pulseDriver.PulseOccurred += (s, e) => this.OnPulse();
        }

        private void OnResponseSelected(Response response)
        {
            this.responsePending = false;

            this.logger.Debug("Response selected: {Response}", response);

            try
            {
                response.Accept();
            }
            catch (MofichanAuthorisationException e)
            {
                this.HandleAuthorisationException(e);
            }

            if (response.Message != null)
            {
                this.backend.OnNext(response.Message);
            }

            this.TryProcessNextMessage();
        }

        private void OnResponseWindowExpired(MessageContext respondingTo)
        {
            this.responsePending = false;

            this.logger.Debug("Response window expired for {Message}", respondingTo.Body);

            this.TryProcessNextMessage();
        }

        private void TryProcessNextMessage()
        {
            /*
             * We should wait for a response to be generated for the last received message
             * (or for the response window to expire) before processing any new messages.
             */
            if (this.responsePending || !this.pendingReceivedMessages.Any())
            {
                return;
            }

            var message = this.pendingReceivedMessages.Dequeue();

            var tags = this.messageClassifierFactory().Classify(message.Body);
            var structuredMessage = message.FromTags(tags);

            this.logger.Debug("Classified {MessageBody} with {Tags}", message.Body, tags);

            var visitor = this.visitorFactory.CreateMessageVisitor(structuredMessage);

            this.rootBehaviour.OnNext(visitor);
            this.responsePending = true;
            this.responseSelector.InspectVisitor(visitor);
            this.HandleAutonomousOutputs(visitor);
        }

        private void HandleAutonomousOutputs(IBehaviourVisitor visitor)
        {
            foreach (var output in visitor.AutonomousOutputs)
            {
                var sideEffects = output.SideEffects.ToList();
                var numSideEffects = sideEffects.Count;
                var message = output.Message;

                this.logger.Debug("Handling autonomous output: {AutonomousOutput}", output);

                try
                {
                    output.Accept();
                }
                catch (MofichanAuthorisationException e)
                {
                    this.HandleAuthorisationException(e);
                }

                if (output.Message != null)
                {
                    this.backend.OnNext(output.Message);
                }
            }
        }

        private void OnReceiveMessage(MessageContext message)
        {
            this.pendingReceivedMessages.Enqueue(message);
            this.TryProcessNextMessage();
        }

        private void OnPulse()
        {
            var visitor = this.visitorFactory.CreatePulseVisitor();

            this.rootBehaviour.OnNext(visitor);
            this.responseSelector.InspectVisitor(visitor);
            this.HandleAutonomousOutputs(visitor);
        }

        private void HandleAuthorisationException(MofichanAuthorisationException e)
        {
            var messageContext = e.MessageContext;

            this.logger.Information(e, "Failed to authorise {User} for request: {Request}",
                messageContext.From, messageContext.Body);

            var sender = messageContext.From as IUser;

            if (sender != null)
            {
                var response = new MessageContext(null, sender,
                    string.Format("I'm afraid you're not authorised to do that, {0}", sender.Name));

                this.backend.OnNext(response);
            }
        }

        /// <summary>
        /// Links behaviours within a collection together to form a chain.
        /// </summary>
        /// <param name="behaviours">The behaviours to link.</param>
        /// <param name="chainBuilder">The object to use to compose the provided behaviours into a chain.</param>
        /// <returns>
        /// The root behaviour in the chain.
        /// </returns>
        private IMofichanBehaviour BuildBehaviourChain(
            IEnumerable<IMofichanBehaviour> behaviours, IBehaviourChainBuilder chainBuilder)
        {
            var behaviourList = behaviours.ToList();

            /*
             * Gives behaviours a chance to inspect/modify
             * other behaviours in the stack before they are
             * wired up and started.
             */
            foreach (var behaviour in behaviours)
            {
                behaviour.InspectBehaviourStack(behaviourList);
            }

            this.behaviours = behaviourList;

            return chainBuilder.BuildChain(behaviourList);
        }
    }
}
