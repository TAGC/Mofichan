using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core.Interfaces;
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
    public class Kernel : IDisposable
    {
        private readonly ILogger logger;

        private IMofichanBackend backend;
        private IMofichanBehaviour rootBehaviour;

        /// <summary>
        /// Initializes a new instance of the <see cref="Kernel"/> class.
        /// </summary>
        /// <param name="name">Mofichan's name (which should of course just be Mofichan).</param>
        /// <param name="backend">The selected backend.</param>
        /// <param name="behaviours">The collection of behaviours to determine Mofichan's personality.</param>
        public Kernel(string name, IMofichanBackend backend, IEnumerable<IMofichanBehaviour> behaviours, ILogger logger)
        {
            Raise.ArgumentNullException.IfIsNull(behaviours, nameof(behaviours));
            Raise.ArgumentException.IfNot(behaviours.Any(), nameof(behaviours),
                string.Format("At least one behaviour must be specified for {0}", name));

            this.logger = logger.ForContext<Kernel>();
            this.logger.Information("Initialising Mofichan with {Backend} and {Behaviours}", backend, behaviours);

            this.backend = backend;
            this.rootBehaviour = BuildBehaviourChain(behaviours);

            this.backend.LinkTo(this.rootBehaviour);
            this.rootBehaviour.LinkTo(this.backend);
        }

        /// <summary>
        /// Starts Mofichan.
        /// <para></para>
        /// This will cause the specified backend and modules in the behaviour chain
        /// to initialise.
        /// </summary>
        public void Start()
        {
            this.backend.Start();
            this.rootBehaviour.Start();
            this.logger.Information("Initialised Mofichan");
        }

        /// <summary>
        /// Links behaviours within a collection together to form a chain.
        /// </summary>
        /// <param name="behaviours">The behaviours to link.</param>
        /// <returns>The root behaviour in the chain.</returns>
        private static IMofichanBehaviour BuildBehaviourChain(IEnumerable<IMofichanBehaviour> behaviours)
        {
            Debug.Assert(behaviours.Any());

            var behaviourList = behaviours.ToList();

            /*
             * Allows behaviours a chance to inspect/modify
             * other behaviours in the stack before they are
             * wired up and started.
             */
            foreach (var behaviour in behaviours)
            {
                behaviour.InspectBehaviourStack(behaviourList);
            }

            for (var i = 0; i < behaviourList.Count - 1; i++)
            {
                var upstreamBehaviour = behaviourList[i];
                var downstreamBehaviour = behaviourList[i + 1];

                upstreamBehaviour.LinkTo<IncomingMessage>(downstreamBehaviour);
                downstreamBehaviour.LinkTo<OutgoingMessage>(upstreamBehaviour);
            }

            return behaviourList[0];
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                this.logger.Information("Disposing Mofichan");
                if (disposing)
                {
                    this.backend.Dispose();

                    /*
                     * Disposing the root behaviour should
                     * propagate disposal down the chain.
                     */
                    this.rootBehaviour.Dispose();
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
