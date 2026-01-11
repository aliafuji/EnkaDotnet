namespace EnkaDotNet.Enums.HSR
{
    public enum ElementType
    {
        Unknown = 0,
        Physical,
        Fire,
        Ice,
        Lightning,
        Wind,
        Quantum,
        Imaginary
    }

    public enum PathType
    {
        Unknown = 0,
        Warrior = 1,  // The Destruction
        Rogue = 2,    // The Hunt
        Mage = 3,     // The Erudition
        Shaman = 4,   // The Harmony
        Warlock = 5,  // The Nihility
        Knight = 6,   // The Preservation
        Priest = 7,   // The Abundance
        Memory = 8    // The Remembrance
    }

    public enum SkillType
    {
        BasicAttack = 0,
        Skill = 1,
        Ultimate = 2,
        Talent = 3,
        Technique = 4,
        SkillTree = 5
    }

    public enum RelicType
    {
        Unknown = 0,
        HEAD = 1,    // Head
        HAND = 2,    // Hands
        BODY = 3,    // Body
        FOOT = 4,    // Feet
        NECK = 5,    // Planar Sphere (Rope)
        OBJECT = 6   // Link Rope (Orbit)
    }

    public enum StatPropertyType
    {
        None = 0,

        // Final Stats (computed)
        MaxHP,                      // MaxHP
        Attack,                     // Attack
        Defence,                    // Defence
        Speed,                      // Speed

        // Base Stats
        BaseHP,                     // BaseHP
        BaseAttack,                 // BaseAttack
        BaseDefence,                // BaseDefence
        BaseSpeed,                  // BaseSpeed

        // Delta Stats (flat bonuses)
        HPDelta,                    // HPDelta
        AttackDelta,                // AttackDelta
        DefenceDelta,               // DefenceDelta
        SpeedDelta,                 // SpeedDelta

        // Ratio Stats (percentage bonuses)
        HPAddedRatio,               // HPAddedRatio
        AttackAddedRatio,           // AttackAddedRatio
        DefenceAddedRatio,          // DefenceAddedRatio
        SpeedAddedRatio,            // SpeedAddedRatio (not commonly used)

        // Critical Stats
        CriticalChance,             // CriticalChance
        CriticalDamage,             // CriticalDamage
        CriticalChanceBase,         // CriticalChanceBase
        CriticalDamageBase,         // CriticalDamageBase

        // Effect Stats
        StatusProbability,          // StatusProbability (Effect Hit Rate)
        StatusResistance,           // StatusResistance (Effect RES)
        StatusProbabilityBase,      // StatusProbabilityBase
        StatusResistanceBase,       // StatusResistanceBase

        // Break Effect
        BreakDamageAddedRatio,      // BreakDamageAddedRatio
        BreakDamageAddedRatioBase,  // BreakDamageAddedRatioBase

        // Healing Stats
        HealRatio,                  // HealRatio (Outgoing Healing Boost)
        HealRatioBase,              // HealRatioBase
        HealTakenRatio,             // HealTakenRatio (Incoming Healing Boost)

        // Energy Stats
        MaxSP,                      // MaxSP (Max Energy)
        SPRatio,                    // SPRatio (Energy Regeneration Rate)
        SPRatioBase,                // SPRatioBase

        // Physical DMG & RES
        PhysicalAddedRatio,         // PhysicalAddedRatio (Physical DMG Boost)
        PhysicalResistance,         // PhysicalResistance
        PhysicalResistanceDelta,    // PhysicalResistanceDelta

        // Fire DMG & RES
        FireAddedRatio,             // FireAddedRatio (Fire DMG Boost)
        FireResistance,             // FireResistance
        FireResistanceDelta,        // FireResistanceDelta

        // Ice DMG & RES
        IceAddedRatio,              // IceAddedRatio (Ice DMG Boost)
        IceResistance,              // IceResistance
        IceResistanceDelta,         // IceResistanceDelta

        // Lightning DMG & RES
        LightningAddedRatio,        // ThunderAddedRatio (Lightning DMG Boost)
        LightningResistance,        // ThunderResistance
        LightningResistanceDelta,   // ThunderResistanceDelta

        // Wind DMG & RES
        WindAddedRatio,             // WindAddedRatio (Wind DMG Boost)
        WindResistance,             // WindResistance
        WindResistanceDelta,        // WindResistanceDelta

        // Quantum DMG & RES
        QuantumAddedRatio,          // QuantumAddedRatio (Quantum DMG Boost)
        QuantumResistance,          // QuantumResistance
        QuantumResistanceDelta,     // QuantumResistanceDelta

        // Imaginary DMG & RES
        ImaginaryAddedRatio,        // ImaginaryAddedRatio (Imaginary DMG Boost)
        ImaginaryResistance,        // ImaginaryResistance
        ImaginaryResistanceDelta    // ImaginaryResistanceDelta
    }

    public enum Rarity
    {
        Unknown = 0,
        Rare = 3,
        VeryRare = 4,
        Epic = 5
    }

    public enum TraceType
    {
        Unknown = 0,
        Stat = 1,        // Stat boost traces
        Skill = 2,       // Basic ATK, Skill, Ultimate, Talent, Technique
        Talent = 3,      // Special talents (there are 3 of these)
        Memosprite = 4   // Memosprite traces
    }

    public static class EnumExtensions
    {
        public static string GetPathName(this PathType pathType)
        {
            switch (pathType)
            {
                case PathType.Warrior: return "Destruction";
                case PathType.Rogue: return "Hunt";
                case PathType.Mage: return "Erudition";
                case PathType.Shaman: return "Harmony";
                case PathType.Warlock: return "Nihility";
                case PathType.Knight: return "Preservation";
                case PathType.Priest: return "Abundance";
                case PathType.Memory: return "Remembrance";
                default: return "Unknown Path";
            }
        }

        public static string GetElementName(this ElementType elementType)
        {
            switch (elementType)
            {
                case ElementType.Physical: return "Physical";
                case ElementType.Fire: return "Fire";
                case ElementType.Ice: return "Ice";
                case ElementType.Lightning: return "Lightning";
                case ElementType.Wind: return "Wind";
                case ElementType.Quantum: return "Quantum";
                case ElementType.Imaginary: return "Imaginary";
                default: return "Unknown Element";
            }
        }

        public static string GetRelicName(this RelicType relicType)
        {
            switch (relicType)
            {
                case RelicType.HEAD: return "Head";
                case RelicType.HAND: return "Hands";
                case RelicType.BODY: return "Body";
                case RelicType.FOOT: return "Feet";
                case RelicType.NECK: return "Planar Sphere";
                case RelicType.OBJECT: return "Link Rope";
                default: return "Unknown Relic Type";
            }
        }

        public static string GetStatPropertyName(this StatPropertyType statType)
        {
            switch (statType)
            {
                case StatPropertyType.MaxHP: return "HP";
                case StatPropertyType.Attack: return "ATK";
                case StatPropertyType.Defence: return "DEF";
                case StatPropertyType.Speed: return "SPD";
                case StatPropertyType.BaseHP: return "Base HP";
                case StatPropertyType.BaseAttack: return "Base ATK";
                case StatPropertyType.BaseDefence: return "Base DEF";
                case StatPropertyType.BaseSpeed: return "Base SPD";
                case StatPropertyType.HPDelta: return "HP";
                case StatPropertyType.AttackDelta: return "ATK";
                case StatPropertyType.DefenceDelta: return "DEF";
                case StatPropertyType.SpeedDelta: return "SPD";
                case StatPropertyType.HPAddedRatio: return "HP%";
                case StatPropertyType.AttackAddedRatio: return "ATK%";
                case StatPropertyType.DefenceAddedRatio: return "DEF%";
                case StatPropertyType.SpeedAddedRatio: return "SPD%";
                case StatPropertyType.CriticalChance:
                case StatPropertyType.CriticalChanceBase: return "CRIT Rate";
                case StatPropertyType.CriticalDamage:
                case StatPropertyType.CriticalDamageBase: return "CRIT DMG";
                case StatPropertyType.StatusProbability:
                case StatPropertyType.StatusProbabilityBase: return "Effect Hit Rate";
                case StatPropertyType.StatusResistance:
                case StatPropertyType.StatusResistanceBase: return "Effect RES";
                case StatPropertyType.BreakDamageAddedRatio:
                case StatPropertyType.BreakDamageAddedRatioBase: return "Break Effect";
                case StatPropertyType.HealRatio:
                case StatPropertyType.HealRatioBase: return "Outgoing Healing";
                case StatPropertyType.HealTakenRatio: return "Incoming Healing";
                case StatPropertyType.MaxSP: return "Max Energy";
                case StatPropertyType.SPRatio:
                case StatPropertyType.SPRatioBase: return "Energy Regen Rate";
                case StatPropertyType.PhysicalAddedRatio: return "Physical DMG";
                case StatPropertyType.FireAddedRatio: return "Fire DMG";
                case StatPropertyType.IceAddedRatio: return "Ice DMG";
                case StatPropertyType.LightningAddedRatio: return "Lightning DMG";
                case StatPropertyType.WindAddedRatio: return "Wind DMG";
                case StatPropertyType.QuantumAddedRatio: return "Quantum DMG";
                case StatPropertyType.ImaginaryAddedRatio: return "Imaginary DMG";
                case StatPropertyType.PhysicalResistance:
                case StatPropertyType.PhysicalResistanceDelta: return "Physical RES";
                case StatPropertyType.FireResistance:
                case StatPropertyType.FireResistanceDelta: return "Fire RES";
                case StatPropertyType.IceResistance:
                case StatPropertyType.IceResistanceDelta: return "Ice RES";
                case StatPropertyType.LightningResistance:
                case StatPropertyType.LightningResistanceDelta: return "Lightning RES";
                case StatPropertyType.WindResistance:
                case StatPropertyType.WindResistanceDelta: return "Wind RES";
                case StatPropertyType.QuantumResistance:
                case StatPropertyType.QuantumResistanceDelta: return "Quantum RES";
                case StatPropertyType.ImaginaryResistance:
                case StatPropertyType.ImaginaryResistanceDelta: return "Imaginary RES";
                default: return "Unknown Stat";
            }
        }

        public static bool IsPercentageStat(this StatPropertyType statType)
        {
            switch (statType)
            {
                case StatPropertyType.HPAddedRatio:
                case StatPropertyType.AttackAddedRatio:
                case StatPropertyType.DefenceAddedRatio:
                case StatPropertyType.SpeedAddedRatio:
                case StatPropertyType.CriticalChance:
                case StatPropertyType.CriticalChanceBase:
                case StatPropertyType.CriticalDamage:
                case StatPropertyType.CriticalDamageBase:
                case StatPropertyType.StatusProbability:
                case StatPropertyType.StatusProbabilityBase:
                case StatPropertyType.StatusResistance:
                case StatPropertyType.StatusResistanceBase:
                case StatPropertyType.BreakDamageAddedRatio:
                case StatPropertyType.BreakDamageAddedRatioBase:
                case StatPropertyType.HealRatio:
                case StatPropertyType.HealRatioBase:
                case StatPropertyType.HealTakenRatio:
                case StatPropertyType.SPRatio:
                case StatPropertyType.SPRatioBase:
                case StatPropertyType.PhysicalAddedRatio:
                case StatPropertyType.FireAddedRatio:
                case StatPropertyType.IceAddedRatio:
                case StatPropertyType.LightningAddedRatio:
                case StatPropertyType.WindAddedRatio:
                case StatPropertyType.QuantumAddedRatio:
                case StatPropertyType.ImaginaryAddedRatio:
                case StatPropertyType.PhysicalResistance:
                case StatPropertyType.FireResistance:
                case StatPropertyType.IceResistance:
                case StatPropertyType.LightningResistance:
                case StatPropertyType.WindResistance:
                case StatPropertyType.QuantumResistance:
                case StatPropertyType.ImaginaryResistance:
                    return true;
                default:
                    return false;
            }
        }
    }
}