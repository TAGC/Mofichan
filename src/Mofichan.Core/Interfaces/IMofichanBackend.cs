using System;

namespace Mofichan.Core.Interfaces
{
    /// <summary>
    /// Represents an object that provides backend support for Mofichan. This includes connection management
    /// as well as sending and receiving messages using a particular platform.
    /// </summary>
    public interface IMofichanBackend : IObservable<IncomingMessage>, IObserver<OutgoingMessage>, IDisposable
    {
        /// <summary>
        /// Initialises the backend.
        /// </summary>
        void Start();

        /// <summary>
        /// Attempts to connect Mofichan to a room.
        /// </summary>
        /// <param name="roomId">The identifier of the room to try to join.</param>
        void Join(string roomId);

        /// <summary>
        /// Attempts to disconnect Mofichan from a room.
        /// </summary>
        /// <param name="roomId">The identifier of the room to try to leave.</param>
        void Leave(string roomId);
    }

    /// <summary>
    /// Represents a chatroom.
    /// <para></para>
    /// Chatrooms will usually consist of multiple users who can send and receive messages
    /// from each other in a shared environment.
    /// </summary>
    /// <seealso cref="Mofichan.Core.Interfaces.IMessageTarget" />
    public interface IRoom : IMessageTarget
    {
        /// <summary>
        /// Gets the room identifier.
        /// </summary>
        /// <value>
        /// The room identifier.
        /// </value>
        string RoomId { get; }

        /// <summary>
        /// Gets the room name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        string Name { get; }

        /// <summary>
        /// Connects Mofichan to this room.
        /// </summary>
        void Join();

        /// <summary>
        /// Disconnects Mofichan from this room.
        /// </summary>
        void Leave();
    }

    /// <summary>
    /// Represents an individual user in a chat environment.
    /// <para></para>
    /// This individual may be a member of one or several <see cref="IRoom"/>. 
    /// </summary>
    public interface IUser : IMessageTarget
    {
        string UserId { get; }
        string Name { get; }
        UserType Type { get; }
        string ToString();
    }

    /// <summary>
    /// Represents an individual who is also a member of a(n) <see cref="IRoom"/>. 
    /// </summary>
    public interface IRoomOccupant : IUser
    {
        IRoom Room { get; }
    }

    /// <summary>
    /// An enumeration of the types of users that exist.
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// Represents Mofichan herself.
        /// </summary>
        Self,

        /// <summary>
        /// Represents a user with administrative privileges with Mofichan.
        /// </summary>
        Adminstrator,

        /// <summary>
        /// Represents a user that is not Mofichan or one of her admins.
        /// </summary>
        NormalUser
    }
}
