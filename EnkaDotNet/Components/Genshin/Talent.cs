namespace EnkaDotNet.Components.Genshin;

public class Talent
{
    public int Id { get; internal set; }
    public string Name { get; internal set; } = string.Empty;
    public int Level { get; internal set; }
    public int BaseLevel { get; internal set; }
    public int ExtraLevel { get; internal set; }
    public string IconUrl { get; internal set; } = string.Empty;
}
