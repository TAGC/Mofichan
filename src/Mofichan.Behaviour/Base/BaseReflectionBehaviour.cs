using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Mofichan.Core;
using Mofichan.Core.Interfaces;

namespace Mofichan.Behaviour.Base
{
    /// <summary>
    /// A type of <see cref="Mofichan.Core.Interfaces.IMofichanBehaviour"/> that
    /// handles <see cref="IncomingMessage"/> instances by inspecting its
    /// methods for those that can potentially handle them.
    /// </summary>
    public abstract class BaseReflectionBehaviour : BaseBehaviour
    {
        private const BindingFlags CandidateBindingFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        private readonly IEnumerable<IObserver<IncomingMessage>> messageHandlers;
        private readonly Queue<OutgoingMessage> outgoingMessageQueue;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseReflectionBehaviour" /> class.
        /// </summary>
        /// <param name="responseBuilderFactory">A factory for instances of <see cref="IResponseBuilder"/>.</param>
        /// <param name="passThroughMessages">If set to <c>true</c>, unhandled messages will automatically
        /// be passed downstream and upstream.</param>
        protected BaseReflectionBehaviour(Func<IResponseBuilder> responseBuilderFactory, bool passThroughMessages = true)
            : base(responseBuilderFactory, passThroughMessages)
        {
            this.outgoingMessageQueue = new Queue<OutgoingMessage>();

            var candidateMethods = this.GetType().GetTypeInfo().GetMethods(CandidateBindingFlags);

            this.messageHandlers = from methodInfo in candidateMethods
                                   where HasValidSignature(methodInfo)
                                   let filters = methodInfo.GetCustomAttributes<BaseIncomingMessageFilterAttribute>()
                                   where filters.Any()
                                   let rootFilter = this.BuildFilterForMethod(filters, methodInfo)
                                   select rootFilter;
        }

        /// <summary>
        /// Determines whether this instance can process the specified incoming message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the incoming message; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanHandleIncomingMessage(IncomingMessage message)
        {
            return this.messageHandlers.Any();
        }

        /// <summary>
        /// Determines whether this instance can process the specified outgoing message.
        /// </summary>
        /// <param name="message">The message to check can be handled.</param>
        /// <returns>
        ///   <c>true</c> if this instance can process the outgoing messagee; otherwise, <c>false</c>.
        /// </returns>
        protected override bool CanHandleOutgoingMessage(OutgoingMessage message)
        {
            return false;
        }

        /// <summary>
        /// Handles the incoming message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleIncomingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void HandleIncomingMessage(IncomingMessage message)
        {
            Debug.Assert(this.outgoingMessageQueue.Count == 0, "The queue should be empty");

            try
            {
                foreach (var handler in this.messageHandlers)
                {
                    handler.OnNext(message);
                }

                /*
                 * Send the message downstream if this instance
                 * hasn't handled it.
                 */
                if (this.outgoingMessageQueue.Count == 0)
                {
                    this.SendDownstream(message);
                    return;
                }

                while (this.outgoingMessageQueue.Any())
                {
                    this.SendUpstream(this.outgoingMessageQueue.Dequeue());
                }
            }
            finally
            {
                this.outgoingMessageQueue.Clear();
            }
        }

        /// <summary>
        /// Handles the outgoing message.
        /// <para></para>
        /// This method will only be invoked if <c>CanHandleOutgoingMessage(message)</c> is <c>true</c>.
        /// </summary>
        /// <param name="message">The message to process.</param>
        protected override void HandleOutgoingMessage(OutgoingMessage message)
        {
            throw new NotImplementedException();
        }

        private static bool HasValidSignature(MethodInfo methodInfo)
        {
            Func<bool> hasSingleParameter =
                () => methodInfo.GetParameters().Length == 1;

            Func<bool> paramIsIncomingMessage =
                () => methodInfo.GetParameters()[0].ParameterType == typeof(IncomingMessage);

            Func<bool> returnsPossibleOutgoingMessage =
                () => methodInfo.ReturnType == typeof(OutgoingMessage?);

            return hasSingleParameter()
                && paramIsIncomingMessage()
                && returnsPossibleOutgoingMessage();
        }

        private BaseIncomingMessageFilterAttribute BuildFilterForMethod(
            IEnumerable<BaseIncomingMessageFilterAttribute> filters,
            MethodInfo method)
        {
            Debug.Assert(filters != null, "Filter list should not be null");
            Debug.Assert(filters.Any(), "Filter list should not be empty");
            Debug.Assert(HasValidSignature(method), "Method has invalid signature");

            var filterList = filters.ToList();

            for (var i = 0; i < filterList.Count - 1; i++)
            {
                filterList[i].Subscribe(filterList[i + 1]);
            }

            filters.Last().Subscribe(message => this.InvokeMethodWithMessage(method, message));

            return filterList[0];
        }

        private void InvokeMethodWithMessage(MethodInfo method, IncomingMessage message)
        {
            Debug.Assert(HasValidSignature(method), "Method has invalid signature");

            var response = method.Invoke(this, new object[] { message }) as OutgoingMessage?;

            if (response.HasValue)
            {
                this.outgoingMessageQueue.Enqueue(response.Value);
            }
        }
    }
}
