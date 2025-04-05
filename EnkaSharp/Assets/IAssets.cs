using EnkaSharp.Enums;

namespace EnkaSharp.Assets
{
    public interface IAssets
    {
        GameType GameType { get; }
        string Language { get; }
        string AssetsPath { get; }
        string GetText(string? hash);
    }
}