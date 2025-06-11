using EnkaDotNet.Enums.Genshin;

namespace EnkaDotNet.Components.Genshin
{
    public class CharacterSummary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public ElementType Element { get; set; } = ElementType.Unknown;

        public static CharacterSummary FromCharacter(Character character) =>
            new CharacterSummary
            {
                Id = character.Id,
                Name = character.Name,
                IconUrl = character.IconUrl,
                Element = character.Element
            };
    }
}