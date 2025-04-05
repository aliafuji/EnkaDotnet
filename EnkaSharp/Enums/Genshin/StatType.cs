namespace EnkaSharp.Enums.Genshin
{
    public enum StatType
    {
        None,

        BaseHP,             // FIGHT_PROP_BASE_HP (ID: 1)
        BaseAttack,         // FIGHT_PROP_BASE_ATTACK (ID: 4)
        BaseDefense,        // FIGHT_PROP_BASE_DEFENSE (ID: 7)

        HPPercentage,       // FIGHT_PROP_HP_PERCENT (ID: 2)
        AttackPercentage,   // FIGHT_PROP_ATTACK_PERCENT (ID: 5)
        DefensePercentage,  // FIGHT_PROP_DEFENSE_PERCENT (ID: 8)
        CriticalRate,       // FIGHT_PROP_CRITICAL (ID: 20)
        CriticalDamage,     // FIGHT_PROP_CRITICAL_HURT (ID: 22)
        EnergyRecharge,     // FIGHT_PROP_CHARGE_EFFICIENCY (ID: 23)
        ElementalMastery,   // FIGHT_PROP_ELEMENT_MASTERY (ID: 28)
        HealingBonus,       // FIGHT_PROP_HEAL_ADD (ID: 26)

        PhysicalDamageBonus,// FIGHT_PROP_PHYSICAL_ADD_HURT (ID: 30)
        AnemoDamageBonus,   // FIGHT_PROP_WIND_ADD_HURT (ID: 44)
        GeoDamageBonus,     // FIGHT_PROP_ROCK_ADD_HURT (ID: 45)
        ElectroDamageBonus, // FIGHT_PROP_ELEC_ADD_HURT (ID: 41)
        HydroDamageBonus,   // FIGHT_PROP_WATER_ADD_HURT (ID: 42)
        PyroDamageBonus,    // FIGHT_PROP_FIRE_ADD_HURT (ID: 40)
        CryoDamageBonus,    // FIGHT_PROP_ICE_ADD_HURT (ID: 46)
        DendroDamageBonus,  // FIGHT_PROP_GRASS_ADD_HURT (ID: 43)

        HP,                 // Final HP (ID: 2000)
        Attack,             // Final Attack (ID: 2001)
        Defense,            // Final Defense (ID: 2002)

        IncomingHealingBonus, // FIGHT_PROP_HEALED_ADD (ID: 27)
        CooldownReduction,    // FIGHT_PROP_SKILL_CD_MINUS_RATIO (ID: ?)
        ShieldStrength,       // FIGHT_PROP_SHIELD_COST_MINUS_RATIO (ID: ?) 
    }
}
