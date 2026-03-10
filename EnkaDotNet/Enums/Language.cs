using System.ComponentModel;
using System.Runtime.Serialization;

namespace EnkaDotNet.Enums
{
    public enum Language
    {
        [EnumMember(Value = "en")]
        English,

        [EnumMember(Value = "ru")]
        Russian,

        [EnumMember(Value = "vi")]
        Vietnamese,

        [EnumMember(Value = "th")]
        Thai,

        [EnumMember(Value = "pt")]
        Portuguese,

        [EnumMember(Value = "ko")]
        Korean,

        [EnumMember(Value = "ja")]
        Japanese,

        [EnumMember(Value = "id")]
        Indonesian,

        [EnumMember(Value = "fr")]
        French,

        [EnumMember(Value = "es")]
        Spanish,

        [EnumMember(Value = "de")]
        German,

        [EnumMember(Value = "zh-tw")]
        TraditionalChinese,

        [EnumMember(Value = "zh-cn")]
        SimplifiedChinese,

        [EnumMember(Value = "it")]
        Italian,

        [EnumMember(Value = "tr")]
        Turkish
    }
}
