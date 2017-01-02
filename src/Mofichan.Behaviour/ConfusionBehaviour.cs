using System;
using System.Linq;
using Mofichan.Behaviour.Base;
using Mofichan.Core.Visitor;

namespace Mofichan.Behaviour
{
    /// <summary>
    /// This <see cref="IMofichanBehaviour"/> causes Mofichan to reply with indications of confusion
    /// if a message is received that she believes is directed at her.
    /// <para></para>
    /// The responses generated have low relevance and will likely lose to other response candidates
    /// if any are made.
    /// </summary>
    public class ConfusionBehaviour : BaseBehaviour
    {
        private static readonly double responseChance = 0.2;

        private readonly Random random = new Random();

        /// <summary>
        /// Invoked when this behaviour is visited by an <see cref="OnMessageVisitor" />.
        /// <para></para>
        /// Subclasses should call the base implementation of this method to pass the
        /// visitor downstream.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        protected override void HandleMessageVisitor(OnMessageVisitor visitor)
        {
            base.HandleMessageVisitor(visitor);

            var tags = visitor.Message.Tags;
            var directedAtMofi = tags.Count() == 1 && tags.ElementAt(0) == "directedAtMofichan";

            if (directedAtMofi && this.random.NextDouble() <= responseChance)
            {
                visitor.RegisterResponse(rb => rb
                    .WithMessage(mb => mb
                        .FromTags(prefix: string.Empty, tags: new[] { "confused" }))
                    .RelevantBecause(it => it.SuitsMessageTags("directedAtMofichan")));
            }
        }
    }
}
