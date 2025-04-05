namespace EnkaSharp.Components.Genshin
{
    public class NameCard
    {
        public string? Id { get; set; }
        public string? Icon { get; set; }
        public NameCard() { }
        public NameCard(int id, string? icon)
        {
            this.Id = id.ToString();
            this.Icon = icon;
        }
    }
}
