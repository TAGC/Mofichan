using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core
{
    public class Kernel : IDisposable
    {
        private IMofichanBackend backend;
        private IMofichanBehaviour rootBehaviour;

        public Kernel(string name, IMofichanBackend backend, IEnumerable<IMofichanBehaviour> behaviours)
        {
            Raise.ArgumentNullException.IfIsNull(behaviours, nameof(behaviours));
            Raise.ArgumentException.IfNot(behaviours.Any(), nameof(behaviours),
                string.Format("At least one behaviour must be specified for {0}", name));

            this.backend = backend;
            this.rootBehaviour = BuildBehaviourChain(behaviours);

            this.backend.LinkTo(this.rootBehaviour);
            this.rootBehaviour.LinkTo(this.backend);
        }

        public void Start()
        {
            this.backend.Start();
            this.rootBehaviour.Start();
        }

        private static IMofichanBehaviour BuildBehaviourChain(IEnumerable<IMofichanBehaviour> behaviours)
        {
            Debug.Assert(behaviours.Any());

            var behaviourArray = behaviours.ToArray();

            for (var i = 0; i < behaviourArray.Length - 1; i++)
            {
                var upstreamBehaviour = behaviourArray[i];
                var downstreamBehaviour = behaviourArray[i + 1];

                upstreamBehaviour.LinkTo<IncomingMessage>(downstreamBehaviour);
                downstreamBehaviour.LinkTo<OutgoingMessage>(upstreamBehaviour);
            }

            return behaviourArray[0];
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
