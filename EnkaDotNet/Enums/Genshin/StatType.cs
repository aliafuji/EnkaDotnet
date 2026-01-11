namespace EnkaDotNet.Enums.Genshin
{
    public enum StatType
    {
        None = 0,

        // Base Stats
        BaseHP = 1,                     // FIGHT_PROP_BASE_HP
        HP_Flat = 2,                    // FIGHT_PROP_HP (flat HP bonus)
        HPPercentage = 3,               // FIGHT_PROP_HP_PERCENT
        BaseAttack = 4,                 // FIGHT_PROP_BASE_ATTACK
        Attack_Flat = 5,                // FIGHT_PROP_ATTACK (flat ATK bonus)
        AttackPercentage = 6,           // FIGHT_PROP_ATTACK_PERCENT
        BaseDefense = 7,                // FIGHT_PROP_BASE_DEFENSE
        Defense_Flat = 8,               // FIGHT_PROP_DEFENSE (flat DEF bonus)
        DefensePercentage = 9,          // FIGHT_PROP_DEFENSE_PERCENT
        BaseSpeed = 10,                 // FIGHT_PROP_BASE_SPEED
        SpeedPercentage = 11,           // FIGHT_PROP_SPEED_PERCENT
        HPMPPercentage = 12,            // FIGHT_PROP_HP_MP_PERCENT
        AttackMPPercentage = 13,        // FIGHT_PROP_ATTACK_MP_PERCENT

        // Critical Stats
        CriticalRate = 20,              // FIGHT_PROP_CRITICAL
        AntiCritical = 21,              // FIGHT_PROP_ANTI_CRITICAL
        CriticalDamage = 22,            // FIGHT_PROP_CRITICAL_HURT

        // Energy & Mastery
        EnergyRecharge = 23,            // FIGHT_PROP_CHARGE_EFFICIENCY
        AddHurt = 24,                   // FIGHT_PROP_ADD_HURT (all damage bonus)
        SubHurt = 25,                   // FIGHT_PROP_SUB_HURT (all damage reduction)
        HealingBonus = 26,              // FIGHT_PROP_HEAL_ADD
        IncomingHealingBonus = 27,      // FIGHT_PROP_HEALED_ADD
        ElementalMastery = 28,          // FIGHT_PROP_ELEMENT_MASTERY

        // Physical Stats
        PhysicalResistance = 29,        // FIGHT_PROP_PHYSICAL_SUB_HURT
        PhysicalDamageBonus = 30,       // FIGHT_PROP_PHYSICAL_ADD_HURT

        // Defense Ignore
        DefenseIgnoreRatio = 31,        // FIGHT_PROP_DEFENCE_IGNORE_RATIO
        DefenseIgnoreDelta = 32,        // FIGHT_PROP_DEFENCE_IGNORE_DELTA

        // Elemental Damage Bonuses
        PyroDamageBonus = 40,           // FIGHT_PROP_FIRE_ADD_HURT
        ElectroDamageBonus = 41,        // FIGHT_PROP_ELEC_ADD_HURT
        HydroDamageBonus = 42,          // FIGHT_PROP_WATER_ADD_HURT
        DendroDamageBonus = 43,         // FIGHT_PROP_GRASS_ADD_HURT
        AnemoDamageBonus = 44,          // FIGHT_PROP_WIND_ADD_HURT
        GeoDamageBonus = 45,            // FIGHT_PROP_ROCK_ADD_HURT
        CryoDamageBonus = 46,           // FIGHT_PROP_ICE_ADD_HURT
        HeadAddHurt = 47,               // FIGHT_PROP_HIT_HEAD_ADD_HURT

        // Elemental Resistances
        PyroResistance = 50,            // FIGHT_PROP_FIRE_SUB_HURT
        ElectroResistance = 51,         // FIGHT_PROP_ELEC_SUB_HURT
        HydroResistance = 52,           // FIGHT_PROP_WATER_SUB_HURT
        DendroResistance = 53,          // FIGHT_PROP_GRASS_SUB_HURT
        AnemoResistance = 54,           // FIGHT_PROP_WIND_SUB_HURT
        GeoResistance = 55,             // FIGHT_PROP_ROCK_SUB_HURT
        CryoResistance = 56,            // FIGHT_PROP_ICE_SUB_HURT

        // Effect Stats
        EffectHit = 60,                 // FIGHT_PROP_EFFECT_HIT
        EffectResist = 61,              // FIGHT_PROP_EFFECT_RESIST
        FreezeResist = 62,              // FIGHT_PROP_FREEZE_RESIST
        DizzyResist = 64,               // FIGHT_PROP_DIZZY_RESIST
        FreezeShorten = 65,             // FIGHT_PROP_FREEZE_SHORTEN
        DizzyShorten = 67,              // FIGHT_PROP_DIZZY_SHORTEN

        // Max Energy
        MaxPyroEnergy = 70,             // FIGHT_PROP_MAX_FIRE_ENERGY
        MaxElectroEnergy = 71,          // FIGHT_PROP_MAX_ELEC_ENERGY
        MaxHydroEnergy = 72,            // FIGHT_PROP_MAX_WATER_ENERGY
        MaxDendroEnergy = 73,           // FIGHT_PROP_MAX_GRASS_ENERGY
        MaxAnemoEnergy = 74,            // FIGHT_PROP_MAX_WIND_ENERGY
        MaxCryoEnergy = 75,             // FIGHT_PROP_MAX_ICE_ENERGY
        MaxGeoEnergy = 76,              // FIGHT_PROP_MAX_ROCK_ENERGY

        // Utility
        CooldownReduction = 80,         // FIGHT_PROP_SKILL_CD_MINUS_RATIO
        ShieldStrength = 81,            // FIGHT_PROP_SHIELD_COST_MINUS_RATIO

        // Current Energy
        CurrentPyroEnergy = 1000,       // FIGHT_PROP_CUR_FIRE_ENERGY
        CurrentElectroEnergy = 1001,    // FIGHT_PROP_CUR_ELEC_ENERGY
        CurrentHydroEnergy = 1002,      // FIGHT_PROP_CUR_WATER_ENERGY
        CurrentDendroEnergy = 1003,     // FIGHT_PROP_CUR_GRASS_ENERGY
        CurrentAnemoEnergy = 1004,      // FIGHT_PROP_CUR_WIND_ENERGY
        CurrentCryoEnergy = 1005,       // FIGHT_PROP_CUR_ICE_ENERGY
        CurrentGeoEnergy = 1006,        // FIGHT_PROP_CUR_ROCK_ENERGY
        CurrentHP = 1010,               // FIGHT_PROP_CUR_HP

        // Final Stats
        HP = 2000,                      // FIGHT_PROP_MAX_HP
        Attack = 2001,                  // FIGHT_PROP_CUR_ATTACK
        Defense = 2002,                 // FIGHT_PROP_CUR_DEFENSE
        Speed = 2003,                   // FIGHT_PROP_CUR_SPEED
        CurrentHPDebts = 2004,          // FIGHT_PROP_CUR_HP_DEBTS
        CurrentHPPaidDebts = 2005,      // FIGHT_PROP_CUR_HP_PAID_DEBTS

        // Non-Extra Stats (base stats without bonuses)
        NonExtraAttack = 3000,          // FIGHT_PROP_NONEXTRA_ATTACK
        NonExtraDefense = 3001,         // FIGHT_PROP_NONEXTRA_DEFENSE
        NonExtraCritical = 3002,        // FIGHT_PROP_NONEXTRA_CRITICAL
        NonExtraAntiCritical = 3003,    // FIGHT_PROP_NONEXTRA_ANTI_CRITICAL
        NonExtraCriticalHurt = 3004,    // FIGHT_PROP_NONEXTRA_CRITICAL_HURT
        NonExtraChargeEfficiency = 3005,// FIGHT_PROP_NONEXTRA_CHARGE_EFFICIENCY
        NonExtraElementMastery = 3006,  // FIGHT_PROP_NONEXTRA_ELEMENT_MASTERY
        NonExtraPhysicalSubHurt = 3007, // FIGHT_PROP_NONEXTRA_PHYSICAL_SUB_HURT
        NonExtraPyroDamageBonus = 3008, // FIGHT_PROP_NONEXTRA_FIRE_ADD_HURT
        NonExtraElectroDamageBonus = 3009, // FIGHT_PROP_NONEXTRA_ELEC_ADD_HURT
        NonExtraHydroDamageBonus = 3010,// FIGHT_PROP_NONEXTRA_WATER_ADD_HURT
        NonExtraDendroDamageBonus = 3011,// FIGHT_PROP_NONEXTRA_GRASS_ADD_HURT
        NonExtraAnemoDamageBonus = 3012,// FIGHT_PROP_NONEXTRA_WIND_ADD_HURT
        NonExtraGeoDamageBonus = 3013,  // FIGHT_PROP_NONEXTRA_ROCK_ADD_HURT
        NonExtraCryoDamageBonus = 3014, // FIGHT_PROP_NONEXTRA_ICE_ADD_HURT
        NonExtraPyroResistance = 3015,  // FIGHT_PROP_NONEXTRA_FIRE_SUB_HURT
        NonExtraElectroResistance = 3016,// FIGHT_PROP_NONEXTRA_ELEC_SUB_HURT
        NonExtraHydroResistance = 3017, // FIGHT_PROP_NONEXTRA_WATER_SUB_HURT
        NonExtraDendroResistance = 3018,// FIGHT_PROP_NONEXTRA_GRASS_SUB_HURT
        NonExtraAnemoResistance = 3019, // FIGHT_PROP_NONEXTRA_WIND_SUB_HURT
        NonExtraGeoResistance = 3020,   // FIGHT_PROP_NONEXTRA_ROCK_SUB_HURT
        NonExtraCryoResistance = 3021,  // FIGHT_PROP_NONEXTRA_ICE_SUB_HURT
        NonExtraCooldownReduction = 3022,// FIGHT_PROP_NONEXTRA_SKILL_CD_MINUS_RATIO
        NonExtraShieldStrength = 3023,  // FIGHT_PROP_NONEXTRA_SHIELD_COST_MINUS_RATIO
        NonExtraPhysicalDamageBonus = 3024, // FIGHT_PROP_NONEXTRA_PHYSICAL_ADD_HURT

        // Elemental Reaction Stats
        ElementalReactionCritRate = 3025,      // FIGHT_PROP_ELEM_REACT_CRITICAL
        ElementalReactionCritDamage = 3026,    // FIGHT_PROP_ELEM_REACT_CRITICAL_HURT
        OverloadedCritRate = 3027,             // FIGHT_PROP_ELEM_REACT_EXPLODE_CRITICAL
        OverloadedCritDamage = 3028,           // FIGHT_PROP_ELEM_REACT_EXPLODE_CRITICAL_HURT
        SwirlCritRate = 3029,                  // FIGHT_PROP_ELEM_REACT_SWIRL_CRITICAL
        SwirlCritDamage = 3030,                // FIGHT_PROP_ELEM_REACT_SWIRL_CRITICAL_HURT
        ElectroChargedCritRate = 3031,         // FIGHT_PROP_ELEM_REACT_ELECTRIC_CRITICAL
        ElectroChargedCritDamage = 3032,       // FIGHT_PROP_ELEM_REACT_ELECTRIC_CRITICAL_HURT
        SuperconductCritRate = 3033,           // FIGHT_PROP_ELEM_REACT_SCONDUCT_CRITICAL
        SuperconductCritDamage = 3034,         // FIGHT_PROP_ELEM_REACT_SCONDUCT_CRITICAL_HURT
        BurnCritRate = 3035,                   // FIGHT_PROP_ELEM_REACT_BURN_CRITICAL
        BurnCritDamage = 3036,                 // FIGHT_PROP_ELEM_REACT_BURN_CRITICAL_HURT
        ShatteredCritRate = 3037,              // FIGHT_PROP_ELEM_REACT_FROZENBROKEN_CRITICAL
        ShatteredCritDamage = 3038,            // FIGHT_PROP_ELEM_REACT_FROZENBROKEN_CRITICAL_HURT
        BloomCritRate = 3039,                  // FIGHT_PROP_ELEM_REACT_OVERGROW_CRITICAL
        BloomCritDamage = 3040,                // FIGHT_PROP_ELEM_REACT_OVERGROW_CRITICAL_HURT
        BurgeonCritRate = 3041,                // FIGHT_PROP_ELEM_REACT_OVERGROW_FIRE_CRITICAL
        BurgeonCritDamage = 3042,              // FIGHT_PROP_ELEM_REACT_OVERGROW_FIRE_CRITICAL_HURT
        HyperbloomCritRate = 3043,             // FIGHT_PROP_ELEM_REACT_OVERGROW_ELECTRIC_CRITICAL
        HyperbloomCritDamage = 3044,           // FIGHT_PROP_ELEM_REACT_OVERGROW_ELECTRIC_CRITICAL_HURT
        BaseElementalReactionCritRate = 3045,  // FIGHT_PROP_BASE_ELEM_REACT_CRITICAL
        BaseElementalReactionCritDamage = 3046 // FIGHT_PROP_BASE_ELEM_REACT_CRITICAL_HURT
    }
}