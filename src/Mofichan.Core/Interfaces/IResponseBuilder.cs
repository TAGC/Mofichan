namespace Mofichan.Core.Interfaces
{
    public interface IResponseBuilder
    {
        IResponseBuilder With(params string[] tags);
        IResponseBuilder With(double chance, params string[] tags);

        IResponseBuilder WithAnyOf(params string[] phrases);
        IResponseBuilder WithAnyOf(double chance, params string[] phrases);

        string Build();
    }
}
