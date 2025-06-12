namespace EnkaDotNet.Enums.ZZZ
{
    public enum StatType
    {
        None = 0,

        // HP Stats
        HPBase = 11101,          // HP [Base]
        HPPercent = 11102,       // HP%
        HPFlat = 11103,          // HP [Flat]

        // ATK Stats
        ATKBase = 12101,         // ATK [Base]
        ATKPercent = 12102,      // ATK%
        ATKFlat = 12103,         // ATK [Flat]

        // Impact Stats
        ImpactBase = 12201,      // Impact [Base]
        ImpactPercent = 12202,   // Impact%

        // DEF Stats
        DefBase = 13101,         // Def [Base]
        DefPercent = 13102,      // Def%
        DefFlat = 13103,         // Def [Flat]

        // Crit Stats
        CritRateBase = 20101,    // Crit Rate [Base]
        CritRateFlat = 20103,    // Crit Rate [Flat]
        CritDMGBase = 21101,     // Crit DMG [Base]
        CritDMGFlat = 21103,     // Crit DMG [Flat]

        // Penetration Stats
        PenRatioBase = 23101,    // Pen Ratio [Base]
        PenRatioFlat = 23103,    // Pen Ratio [Flat]
        PENBase = 23201,         // PEN [Base]
        PENFlat = 23203,         // PEN [Flat]

        // Energy Stats
        EnergyRegenBase = 30501,    // Energy Regen [Base]
        EnergyRegenPercent = 30502, // Energy Regen%
        EnergyRegenFlat = 30503,    // Energy Regen [Flat]

        // Anomaly Stats
        AnomalyProficiencyBase = 31201, // Anomaly Proficiency [Base]
        AnomalyProficiencyFlat = 31203, // Anomaly Proficiency [Flat]
        AnomalyMasteryBase = 31401,     // Anomaly Mastery [Base]
        AnomalyMasteryPercent = 31402,  // Anomaly Mastery%
        AnomalyMasteryFlat = 31403,     // Anomaly Mastery [Flat]

        // Elemental Damage Bonuses
        PhysicalDMGBonusBase = 31501, // Physical DMG Bonus [Base]
        PhysicalDMGBonusFlat = 31503, // Physical DMG Bonus [Flat]
        FireDMGBonusBase = 31601,     // Fire DMG Bonus [Base]
        FireDMGBonusFlat = 31603,     // Fire DMG Bonus [Flat]
        IceDMGBonusBase = 31701,      // Ice DMG Bonus [Base]
        IceDMGBonusFlat = 31703,      // Ice DMG Bonus [Flat]
        ElectricDMGBonusBase = 31801, // Electric DMG Bonus [Base]
        ElectricDMGBonusFlat = 31803, // Electric DMG Bonus [Flat]
        EtherDMGBonusBase = 31901,    // Ether DMG Bonus [Base]
        EtherDMGBonusFlat = 31903,     // Ether DMG Bonus [Flat]

        // Rupture Agent Specific Stats
        AutomaticAdrenalineAccumulationBase = 32001, // Automatic Adrenaline Accumulation [Base]
        AutomaticAdrenalineAccumulationPercent = 32002, // Automatic Adrenaline Accumulation%
        AutomaticAdrenalineAccumulationFlat = 32003, // Automatic Adrenaline Accumulation [Flat]
    }
}