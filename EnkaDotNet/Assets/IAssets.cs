using EnkaDotNet.Enums;

namespace EnkaDotNet.Assets
{
    public interface IAssets
    {
        GameType GameType { get; }
        string Language { get; }
        string GetText(string? hash);
    }
}