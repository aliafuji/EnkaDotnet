namespace EnkaDotNet.Components.Genshin
{
    public class Profile
    {
        public int Id { get; set; }
        public string? Icon { get; set; }

        public static Profile FromAssets(int id, string? icon) =>
            new Profile { Id = id, Icon = icon };
    }
}