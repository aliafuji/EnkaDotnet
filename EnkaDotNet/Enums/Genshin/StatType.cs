namespace EnkaDotNet.Enums.Genshin
{
    public enum StatType
    {
        None,

        // Base Stats
        BaseHP,             // FIGHT_PROP_BASE_HP (ID: 1)
        BaseAttack,         // FIGHT_PROP_BASE_ATTACK (ID: 4)
        BaseDefense,        // FIGHT_PROP_BASE_DEFENSE (ID: 7)
        BaseSpeed,          // FIGHT_PROP_BASE_SPEED (ID: 10)

        // Percentage Stats
        HPPercentage,       // FIGHT_PROP_HP_PERCENT (ID: 2)
        AttackPercentage,   // FIGHT_PROP_ATTACK_PERCENT (ID: 5)
        DefensePercentage,  // FIGHT_PROP_DEFENSE_PERCENT (ID: 8)
        SpeedPercentage,    // FIGHT_PROP_SPEED_PERCENT (ID: 11)

        // Critical Stats
        CriticalRate,       // FIGHT_PROP_CRITICAL (ID: 20)
        CriticalDamage,     // FIGHT_PROP_CRITICAL_HURT (ID: 22)

        // Energy & Mastery
        EnergyRecharge,     // FIGHT_PROP_CHARGE_EFFICIENCY (ID: 23)
        ElementalMastery,   // FIGHT_PROP_ELEMENT_MASTERY (ID: 28)

        // Healing
        HealingBonus,       // FIGHT_PROP_HEAL_ADD (ID: 26)
        IncomingHealingBonus, // FIGHT_PROP_HEALED_ADD (ID: 27)

        // Resistances
        PhysicalResistance, // FIGHT_PROP_PHYSICAL_SUB_HURT (ID: 29)
        PyroResistance,     // FIGHT_PROP_FIRE_SUB_HURT (ID: 50)
        ElectroResistance,  // FIGHT_PROP_ELEC_SUB_HURT (ID: 51)
        HydroResistance,    // FIGHT_PROP_WATER_SUB_HURT (ID: 52)
        DendroResistance,   // FIGHT_PROP_GRASS_SUB_HURT (ID: 53)
        AnemoResistance,    // FIGHT_PROP_WIND_SUB_HURT (ID: 54)
        GeoResistance,      // FIGHT_PROP_ROCK_SUB_HURT (ID: 55)
        CryoResistance,     // FIGHT_PROP_ICE_SUB_HURT (ID: 56)

        // Damage Bonuses
        PhysicalDamageBonus,// FIGHT_PROP_PHYSICAL_ADD_HURT (ID: 30)
        PyroDamageBonus,    // FIGHT_PROP_FIRE_ADD_HURT (ID: 40)
        ElectroDamageBonus, // FIGHT_PROP_ELEC_ADD_HURT (ID: 41)
        HydroDamageBonus,   // FIGHT_PROP_WATER_ADD_HURT (ID: 42)
        DendroDamageBonus,  // FIGHT_PROP_GRASS_ADD_HURT (ID: 43)
        AnemoDamageBonus,   // FIGHT_PROP_WIND_ADD_HURT (ID: 44)
        GeoDamageBonus,     // FIGHT_PROP_ROCK_ADD_HURT (ID: 45)
        CryoDamageBonus,    // FIGHT_PROP_ICE_ADD_HURT (ID: 46)

        // Energy Costs
        PyroEnergyCost,     // FIGHT_PROP_FIRE_ENERGY_COST (ID: 70)
        ElectroEnergyCost,  // FIGHT_PROP_ELEC_ENERGY_COST (ID: 71)
        HydroEnergyCost,    // FIGHT_PROP_WATER_ENERGY_COST (ID: 72)
        DendroEnergyCost,   // FIGHT_PROP_GRASS_ENERGY_COST (ID: 73)
        AnemoEnergyCost,    // FIGHT_PROP_WIND_ENERGY_COST (ID: 74)
        CryoEnergyCost,     // FIGHT_PROP_ICE_ENERGY_COST (ID: 75)
        GeoEnergyCost,      // FIGHT_PROP_ROCK_ENERGY_COST (ID: 76)
        MaxSpecialEnergy,   // FIGHT_PROP_MAX_SP (ID: 77)
        SpecialEnergyCost,  // FIGHT_PROP_SP_COST (ID: 78)

        // Current Energy
        CurrentPyroEnergy,  // FIGHT_PROP_CUR_FIRE_ENERGY (ID: 1000)
        CurrentElectroEnergy, // FIGHT_PROP_CUR_ELEC_ENERGY (ID: 1001)
        CurrentHydroEnergy, // FIGHT_PROP_CUR_WATER_ENERGY (ID: 1002)
        CurrentDendroEnergy, // FIGHT_PROP_CUR_GRASS_ENERGY (ID: 1003)
        CurrentAnemoEnergy, // FIGHT_PROP_CUR_WIND_ENERGY (ID: 1004)
        CurrentCryoEnergy,  // FIGHT_PROP_CUR_ICE_ENERGY (ID: 1005)
        CurrentGeoEnergy,   // FIGHT_PROP_CUR_ROCK_ENERGY (ID: 1006)
        CurrentSpecialEnergy, // FIGHT_PROP_CUR_SP (ID: 1007)
        CurrentHP,          // FIGHT_PROP_CUR_HP (ID: 1010)

        // Final Stats
        HP,                 // FIGHT_PROP_MAX_HP (ID: 2000)
        Attack,             // FIGHT_PROP_CUR_ATTACK (ID: 2001)
        Defense,            // FIGHT_PROP_CUR_DEFENSE (ID: 2002)
        Speed,              // FIGHT_PROP_CUR_SPEED (ID: 2003)

        // Utility
        CooldownReduction,  // FIGHT_PROP_SKILL_CD_MINUS_RATIO (ID: 80)
        ShieldStrength,     // FIGHT_PROP_SHIELD_COST_MINUS_RATIO (ID: 81)

        // Elemental Reaction Stats
        ElementalReactionCritRate,      // FIGHT_PROP_ELEMENT_REACTION_CRIT_RATE (ID: 3025)
        ElementalReactionCritDamage,    // FIGHT_PROP_ELEMENT_REACTION_CRIT_DMG (ID: 3026)
        OverloadedCritRate,             // ID: 3027
        OverloadedCritDamage,           // ID: 3028
        SwirlCritRate,                  // ID: 3029
        SwirlCritDamage,                // ID: 3030
        ElectroChargedCritRate,         // ID: 3031
        ElectroChargedCritDamage,       // ID: 3032
        SuperconductCritRate,           // ID: 3033
        SuperconductCritDamage,         // ID: 3034
        BurnCritRate,                   // ID: 3035
        BurnCritDamage,                 // ID: 3036
        ShatteredCritRate,              // ID: 3037
        ShatteredCritDamage,            // ID: 3038
        BloomCritRate,                  // ID: 3039
        BloomCritDamage,                // ID: 3040
        BurgeonCritRate,                // ID: 3041
        BurgeonCritDamage,              // ID: 3042
        HyperbloomCritRate,             // ID: 3043
        HyperbloomCritDamage,           // ID: 3044
        BaseElementalReactionCritRate,  // ID: 3045
        BaseElementalReactionCritDamage // ID: 3046
    }
}