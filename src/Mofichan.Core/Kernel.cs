using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;

namespace Mofichan.Core
{
    public class Kernel
    {
        private IMofichanBackend backend;
        private IMofichanBehaviour[] behaviours;

        public Kernel(string name, IMofichanBackend backend, IEnumerable<IMofichanBehaviour> behaviours)
        {
            Raise.ArgumentNullException.IfIsNull(behaviours, nameof(behaviours));
            Raise.ArgumentException.IfNot(behaviours.Any(), nameof(behaviours),
                string.Format("At least one behaviour must be specified for {0}", name));

            this.backend = backend;
            this.behaviours = behaviours.ToArray();

            this.backend.LinkTo(this.behaviours[0]);
            this.behaviours[0].LinkTo(this.backend);
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
