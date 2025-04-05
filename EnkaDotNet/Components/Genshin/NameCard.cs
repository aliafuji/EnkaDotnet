namespace EnkaDotNet.Components.Genshin
{
    public class NameCard
    {
        public string? Id { get; set; }
        public string? Icon { get; set; }
        public NameCard() { }
        public NameCard(int id, string? icon)
        {
            Id = id.ToString();
            Icon = icon;
        }
    }
}
