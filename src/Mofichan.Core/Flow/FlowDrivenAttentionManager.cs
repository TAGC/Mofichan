using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mofichan.Core.Interfaces;
using PommaLabs.Thrower;
using Serilog;

namespace Mofichan.Core.Flow
{
    /// <summary>
    /// An implementation of <see cref="IAttentionManager"/> that maintains attention to
    /// users for a discrete number of "steps" determined by sampling from a Gaussian distribution.
    /// <para></para>
    /// Instances of this class use a(n) <see cref="IFlowDriver"/> to determine when to transition to the next step. 
    /// </summary>
    public class FlowDrivenAttentionManager : IAttentionManager, IDisposable
    {
        private readonly double mu;
        private readonly double sigma;
        private readonly IFlowDriver flowDriver;
        private readonly ILogger logger;
        private readonly IDictionary<IUser, int> attentionSpans;
        private readonly Random random;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowDrivenAttentionManager" /> class.
        /// </summary>
        /// <param name="mu">The mean of the attention-duration Gaussian distribution.</param>
        /// <param name="sigma">The standard deviation of the attention-duration Gaussian distribution.</param>
        /// <param name="flowDriver">The flow driver to determine when to transition between steps.</param>
        /// <param name="logger">The logger to use.</param>
        public FlowDrivenAttentionManager(double mu, double sigma, IFlowDriver flowDriver, ILogger logger)
        {
            Raise.ArgumentNullException.IfIsNull(flowDriver, nameof(flowDriver), "The flow driver cannot be null");
            Raise.ArgumentNullException.IfIsNull(logger, nameof(logger), "The logger cannot be null");
            Raise.ArgumentException.If(sigma < 0, nameof(sigma), "Standard deviation must be non-negative");
            Raise.ArgumentNullException.If(double.IsNaN(sigma), "Standard deviation must be a valid real number");

            this.mu = mu;
            this.sigma = sigma;
            this.flowDriver = flowDriver;
            this.logger = logger.ForContext<FlowDrivenAttentionManager>();
            this.attentionSpans = new Dictionary<IUser, int>();
            this.random = new Random();

            this.flowDriver.OnNextStep += StepAttentionSpan;

            this.logger.Debug("Instantiated flow-driven attention manager with mu={Mu}, sigma={Sigma}", mu, sigma);
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
            int attentionDuration = GetRandomAttentionDuration();

            this.attentionSpans[user] = attentionDuration;

            this.logger.Verbose("Renewed attention to {User} for {Duration} steps",
                user.Name, attentionDuration);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.flowDriver.OnNextStep -= this.StepAttentionSpan;
        }

        /// <summary>
        /// Samples the Gaussian distribution configured for this instance to generate the initial duration of
        /// an attention span Mofichan has towards a particular user.
        /// </summary>
        /// <returns>An initial attention span duration in steps.</returns>
        private int GetRandomAttentionDuration()
        {
            double p = this.random.NextDouble();
            int steps = (int)Math.Round(GaussianQuantile(p, this.mu, this.sigma, DistributionTail.Left));

            return steps;
        }

        /// <summary>
        /// Shortens the remaining attention span Mofichan has to all users and makes her stop paying
        /// attention to users when the corresponding attention span is zero.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void StepAttentionSpan(object sender, EventArgs e)
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

        /// <summary>
        /// Implements the quantile (inverse CDF) function for a normal distribution parameterised with
        /// mean <paramref name="mu"/> and standard deviation <paramref name="sigma"/>.
        /// </summary>
        /// <param name="p">The probability.</param>
        /// <param name="mu">The mean of the normal distribution.</param>
        /// <param name="sigma">The standard deviation of the normal distribution.</param>
        /// <param name="tail">Which tail of the distribution to calculate the z-score for.</param>
        /// <returns>if calculating the left tail value, <c>x s.t. P(X &lt;= x) = p</c>;
        /// otherwise <c>x s.t. P(X &gt; x) = p</c> where <c>x ~ N(mu,sigma^2)</c></returns>
        /// <remarks>
        /// Implementation derived from <see href="https://svn.r-project.org/R/trunk/src/nmath/qnorm.c"/>
        /// </remarks>
        private static double GaussianQuantile(double p, double mu, double sigma, DistributionTail tail)
        {
            Raise.ArgumentNullException.IfIsNull(tail, nameof(tail), "Distribution tail must be specified");
            Raise.ArgumentException.If(p < 0 || p > 1, nameof(p), "Probability must be in range [0,1]");
            Raise.ArgumentException.If(sigma < 0, nameof(sigma), "Standard deviation must be non-negative");
            Raise.ArgumentNullException.If(double.IsNaN(sigma), "Standard deviation must be a valid real number");

            bool isRightTail = tail == DistributionTail.Right;

            /*
             * Boundary cases.
             */
            if (sigma == 0)
            {
                return mu;
            }
            else if (p == 0.0)
            {
                return isRightTail ? double.PositiveInfinity : double.NegativeInfinity;
            }
            else if (p == 1.0)
            {
                return isRightTail ? double.NegativeInfinity : double.PositiveInfinity;
            }

            double p_ = isRightTail ? 1 - p : p;
            double q = p_ - 0.5;
            double r;
            double val;

            if (Math.Abs(q) <= 0.425)  /* 0.075 <= p <= 0.925 */
            {
                r = .180625 - q * q;
                val = q * (((((((r * 2509.0809287301226727 +
                           33430.575583588128105) * r + 67265.770927008700853) * r +
                         45921.953931549871457) * r + 13731.693765509461125) * r +
                       1971.5909503065514427) * r + 133.14166789178437745) * r +
                     3.387132872796366608)
                / (((((((r * 5226.495278852854561 +
                         28729.085735721942674) * r + 39307.89580009271061) * r +
                       21213.794301586595867) * r + 5394.1960214247511077) * r +
                     687.1870074920579083) * r + 42.313330701600911252) * r + 1.0);
            }
            else  /* closer than 0.075 from {0,1} boundary */
            {
                r = q > 0 ? (isRightTail ? p : 1 - p) : p_;
                r = Math.Sqrt(-Math.Log(r));


                if (r <= 5)  /* <==> min(p,1-p) >= exp(-25) ~= 1.3888e-11 */
                {
                    r -= 1.6;
                    val = (((((((r * 7.7454501427834140764e-4 +
                            .0227238449892691845833) * r + .24178072517745061177) *
                          r + 1.27045825245236838258) * r +
                         3.64784832476320460504) * r + 5.7694972214606914055) *
                       r + 4.6303378461565452959) * r +
                      1.42343711074968357734)
                     / (((((((r *
                              1.05075007164441684324e-9 + 5.475938084995344946e-4) *
                             r + .0151986665636164571966) * r +
                            .14810397642748007459) * r + .68976733498510000455) *
                          r + 1.6763848301838038494) * r +
                         2.05319162663775882187) * r + 1.0);
                }
                else  /* very close to  0 or 1 */
                {
                    r -= 5.0;
                    val = (((((((r * 2.01033439929228813265e-7 +
                            2.71155556874348757815e-5) * r +
                           .0012426609473880784386) * r + .026532189526576123093) *
                         r + .29656057182850489123) * r +
                        1.7848265399172913358) * r + 5.4637849111641143699) *
                      r + 6.6579046435011037772)
                     / (((((((r *
                              2.04426310338993978564e-15 + 1.4215117583164458887e-7) *
                             r + 1.8463183175100546818e-5) * r +
                            7.868691311456132591e-4) * r + .0148753612908506148525)
                          * r + .13692988092273580531) * r +
                         .59983220655588793769) * r + 1.0);
                }

                if (q < 0.0)
                {
                    val = -val;
                }
            }

            return mu + (sigma * val);
        }

        private enum DistributionTail
        {
            Left,
            Right
        }
    }
}
