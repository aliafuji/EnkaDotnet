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
        None = 0,

        // Base Stats
        HPDelta = 1,
        HPAddedRatio = 2,
        AttackDelta = 3,
        AttackAddedRatio = 4,
        DefenceDelta = 5,
        DefenceAddedRatio = 6,
        SpeedDelta = 7,

        // Critical Stats
        CriticalChance = 8,
        CriticalDamage = 9,

        // Effect Stats
        StatusProbability = 10,
        StatusResistance = 11,
        BreakDamageAddedRatio = 12,

        // Base properties
        BaseHP = 1001,
        BaseAttack = 1002,
        BaseDefence = 1003,
        BaseSpeed = 1004,

        // Elemental DMG
        PhysicalAddedRatio = 2001,
        FireAddedRatio = 2002,
        IceAddedRatio = 2003,
        LightningAddedRatio = 2004,
        WindAddedRatio = 2005,
        QuantumAddedRatio = 2006,
        ImaginaryAddedRatio = 2007,

        // Special stats
        HealRatioBase = 3001,
        SPRatioBase = 3002,
        CriticalChanceBase = 3003,
        CriticalDamageBase = 3004,
        BreakDamageAddedRatioBase = 3005
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