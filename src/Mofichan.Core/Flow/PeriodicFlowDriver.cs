using System;
using System.Threading;
using System.Threading.Tasks;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// A type of <see cref="IFlowDriver"/> that signals <see cref="IFlow"/> instances to
    /// perform their next steps at regular intervals.
    /// </summary>
    public class PeriodicFlowDriver : IFlowDriver, IDisposable
    {
        private readonly ILogger logger;
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Task stepperTask;

        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodicFlowDriver"/> class.
        /// </summary>
        /// <param name="stepPeriod">The period between each step signal.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentException">Thrown if the step period is zero.</exception>
        public PeriodicFlowDriver(TimeSpan stepPeriod, ILogger logger)
        {
            Raise.ArgumentNullException.IfIsNull(logger, nameof(logger));
            Raise.ArgumentException.If(stepPeriod.Equals(TimeSpan.Zero), nameof(stepPeriod),
                "The period cannot be zero");

            this.logger = logger.ForContext<PeriodicFlowDriver>();
            this.cancellationTokenSource = new CancellationTokenSource();
            this.stepperTask = this.StepPeriodicallyAsync(stepPeriod, this.cancellationTokenSource.Token);
        }

        /// <summary>
        /// Occurs when flows should perform their next step.
        /// </summary>
        public event EventHandler OnNextStep;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.cancellationTokenSource.Cancel();
            this.stepperTask.Wait();
            this.disposed = true;
        }

        private async Task StepPeriodicallyAsync(TimeSpan stepPeriod, CancellationToken token)
        {
            long iteration = 0;

            while (!token.IsCancellationRequested)
            {
                iteration++;

                try
                {
                    await Task.Delay(stepPeriod, token);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                this.OnNextStep?.Invoke(this, EventArgs.Empty);

                if (iteration % 100 == 0)
                {
                    this.logger.Debug("Flow step: {Iteration}", iteration);
                }
            }
        }
    }
}
