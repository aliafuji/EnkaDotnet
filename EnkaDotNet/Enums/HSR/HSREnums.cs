using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
        Warrior = 1, // The Destruction
        Rogue = 2,   // The Hunt
        Mage = 3,    // The Erudition
        Shaman = 4,  // The Harmony
        Warlock = 5, // The Nihility
        Knight = 6,  // The Preservation
        Priest = 7,   // The Abundance
        Memory = 8, // The Remembrance
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
        HEAD,    // Head
        HAND,    // Hands
        BODY,    // Body
        FOOT,    // Feet
        NECK,    // Planar Sphere
        OBJECT   // Link Rope
    }

    public enum StatPropertyType
    {
        None,

        // Base Stats
        HPDelta,
        HPAddedRatio,
        AttackDelta,
        AttackAddedRatio,
        DefenceDelta,
        DefenceAddedRatio,
        SpeedDelta,

        // Critical Stats
        CriticalChance,
        CriticalDamage,

        // Effect Stats
        StatusProbability,
        StatusResistance,
        BreakDamageAddedRatio,

        // Base properties
        BaseHP,
        BaseAttack,
        BaseDefence,
        BaseSpeed,

        // Elemental DMG
        PhysicalAddedRatio,
        FireAddedRatio,
        IceAddedRatio,
        LightningAddedRatio,
        WindAddedRatio,
        QuantumAddedRatio,
        ImaginaryAddedRatio,

        // Special stats
        HealRatioBase,
        SPRatioBase,
        CriticalChanceBase,
        CriticalDamageBase,
        BreakDamageAddedRatioBase,
        SpeedAddedRatio,
        StatusResistanceBase
    }

    public enum Rarity
    {
        Unknown = 0,
        Rare = 3,
        VeryRare = 4,
        Epic = 5
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
    }
}