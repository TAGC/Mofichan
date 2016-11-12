namespace Mofichan.Core.Interfaces
{
    public interface IMofichanBackend : IMessageContextHandler
    {
        void Join(string roomId);
        void Leave(string roomId);
    }

    public interface IRoom : IMessageTarget
    {
        string RoomId { get; }
        string Name { get; }
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
