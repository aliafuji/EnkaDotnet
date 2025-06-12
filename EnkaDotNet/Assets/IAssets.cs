namespace EnkaDotNet.Assets
{
    public interface IAssets
    {
        string Language { get; }
        string GameIdentifier { get; }
        string GetText(string hash);
    }
}
