using System;
using System.Threading.Tasks.Dataflow;

namespace Mofichan.Core.Interfaces
{
    public interface IMofichanBackend : IPropagatorBlock<OutgoingMessage, IncomingMessage>, IDisposable
    {
        void Start();
        void Join(string roomId);
        void Leave(string roomId);
    }

    public interface IRoom : IMessageTarget
    {
        string RoomId { get; }
        string Name { get; }

        void Join();
        void Leave();
    }

    public interface IUser : IMessageTarget
    {
        string UserId { get; }
        string Name { get; }
        UserType Type { get; }
    }

    public interface IRoomOccupant : IUser
    {
        IRoom Room { get; }
    }

    public enum UserType
    {
        Self,
        Adminstrator,
        NormalUser
    }
}
