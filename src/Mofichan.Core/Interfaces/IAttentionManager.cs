namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents objects that manage Mofichan's attention towards particular users.
    /// <para></para>
    /// When Mofichan is paying attention to a user, she is more likely to assume that
    /// any messages they say are directed at her.
    /// </summary>
    public interface IAttentionManager
    {
        /// <summary>
        /// Determines whether Mofichan is paying attention to the specified user.
        /// </summary>
        /// <param name="user">The user to check if attention is being paid to.</param>
        /// <returns>
        ///   <c>true</c> if Mofichan is paying attention to the user; otherwise, <c>false</c>.
        /// </returns>
        bool IsPayingAttentionToUser(IUser user);

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
        void RenewAttentionTowardsUser(IUser user);

        /// <summary>
        /// Causes Mofichan to immediately lose attention towards a particular user.
        /// <para></para>
        /// Calls to this method are nullipotent if Mofichan wasn't already paying attention
        /// to the specified user.
        /// </summary>
        /// <param name="user">The user to stop paying attention to.</param>
        void LoseAttentionTowardsUser(IUser user);

        /// <summary>
        /// Causes Mofichan to stop paying attention to all users.
        /// </summary>
        void LoseAttentionToAllUsers();
    }
}
