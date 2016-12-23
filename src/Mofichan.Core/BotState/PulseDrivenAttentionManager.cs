using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mofichan.Core.Interfaces;
using Mofichan.Core.Utility;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.Core.BotState
{
    /// <summary>
    /// An implementation of <see cref="IAttentionManager"/> that maintains attention to
    /// users for a discrete number of "steps" determined by sampling from a Gaussian distribution.
    /// <para></para>
    /// Instances of this class use a(n) <see cref="IPulseDriver"/> to determine how long attention
    /// should be paid to a particular user.
    /// </summary>
    public class PulseDrivenAttentionManager : IAttentionManager, IDisposable
    {
        private readonly double mu;
        private readonly double sigma;
        private readonly IPulseDriver pulseDriver;
        private readonly ILogger logger;
        private readonly IDictionary<IUser, int> attentionSpans;
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="PulseDrivenAttentionManager" /> class.
        /// </summary>
        /// <param name="mu">The mean of the attention-duration Gaussian distribution.</param>
        /// <param name="sigma">The standard deviation of the attention-duration Gaussian distribution.</param>
        /// <param name="pulseDriver">The pulse driver to determine the rate of attention loss.</param>
        /// <param name="logger">The logger to use.</param>
        public PulseDrivenAttentionManager(double mu, double sigma, IPulseDriver pulseDriver, ILogger logger)
        {
            Raise.ArgumentNullException.IfIsNull(pulseDriver, nameof(pulseDriver), "The pulse driver cannot be null");
            Raise.ArgumentNullException.IfIsNull(logger, nameof(logger), "The logger cannot be null");
            Raise.ArgumentException.If(sigma < 0, nameof(sigma), "Standard deviation must be non-negative");
            Raise.ArgumentNullException.If(double.IsNaN(sigma), "Standard deviation must be a valid real number");

            this.mu = mu;
            this.sigma = sigma;
            this.pulseDriver = pulseDriver;
            this.logger = logger.ForContext<PulseDrivenAttentionManager>();
            this.attentionSpans = new Dictionary<IUser, int>();
            this.random = new Random();
            this.pulseDriver.PulseOccurred += this.StepAttentionSpan;

            this.logger.Debug("Instantiated pulse-driven attention manager with mu={Mu}, sigma={Sigma}", mu, sigma);
        }

        /// <summary>
        /// Determines whether Mofichan is paying attention to the specified user.
        /// </summary>
        /// <param name="user">The user to check if attention is being paid to.</param>
        /// <returns>
        ///   <c>true</c> if Mofichan is paying attention to the user; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPayingAttentionToUser(IUser user)
        {
            return this.attentionSpans.ContainsKey(user);
        }

        /// <summary>
        /// Causes Mofichan to stop paying attention to all users.
        /// </summary>
        public void LoseAttentionToAllUsers()
        {
            foreach (var user in this.attentionSpans.Keys.ToList())
            {
                this.LoseAttentionTowardsUser(user);
            }

            Debug.Assert(!this.attentionSpans.Any(), "Attention should have been lost to all users");
        }

        /// <summary>
        /// Causes Mofichan to immediately lose attention towards a particular user.
        /// <para></para>
        /// Calls to this method are nullipotent if Mofichan wasn't already paying attention
        /// to the specified user.
        /// </summary>
        /// <param name="user">The user to stop paying attention to.</param>
        public void LoseAttentionTowardsUser(IUser user)
        {
            this.attentionSpans.Remove(user);

            this.logger.Verbose("Stopped paying attention to {User}", user.Name);
        }

        /// <summary>
        /// Renews Mofichan's attention towards a particular user.
        /// </summary>
        /// <param name="user">The user to renew attention towards.</param>
        /// <remarks>
        /// If Mofichan was not paying attention to the user before, this method causes her
        /// to start paying attention to that user.
        /// <para></para>
        /// If Mofichan was already paying attention to the user, this method potentially
        /// extends the time that she will remain paying attention to the user.
        /// </remarks>
        public void RenewAttentionTowardsUser(IUser user)
        {
            int attentionDuration = this.GetRandomAttentionDuration();

            this.attentionSpans[user] = attentionDuration;

            this.logger.Verbose("Renewed attention to {User} for {Duration} steps",
                user.Name, attentionDuration);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.pulseDriver.PulseOccurred -= this.StepAttentionSpan;
        }

        /// <summary>
        /// Samples the Gaussian distribution configured for this instance to generate the initial duration of
        /// an attention span Mofichan has towards a particular user.
        /// </summary>
        /// <returns>An initial attention span duration in steps.</returns>
        private int GetRandomAttentionDuration()
        {
            return (int)this.random.SampleGaussian(this.mu, this.sigma);
        }

        /// <summary>
        /// Shortens the remaining attention span Mofichan has to all users and makes her stop paying
        /// attention to users when the corresponding attention span is zero.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void StepAttentionSpan(object sender, EventArgs eventArgs)
        {
            foreach (var userIds in this.attentionSpans.Keys.ToList())
            {
                this.attentionSpans[userIds]--;
            }

            var stopPayingAttentionTo = from pair in this.attentionSpans
                                        where pair.Value == 0
                                        select pair.Key;

            foreach (var user in stopPayingAttentionTo.ToList())
            {
                this.LoseAttentionTowardsUser(user);
            }
        }
    }
}
