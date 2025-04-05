namespace EnkaDotNet.Components.Genshin;

public class Constellation
{
    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public string IconUrl { get; internal set; } = string.Empty;
    public int Position { get; internal set; }
}
