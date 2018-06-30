using Modding;

namespace redwing
{
    public static class version_info
    {
        public const int SETTINGS_VER = 10;
    }

    public class redwing_global_settings : IModSettings
    {


        public void reset()
        {
            BoolValues.Clear();
            IntValues.Clear();
            FloatValues.Clear();
            StringValues.Clear();

            useGreymothDashWhenBlackmothMissing = true;
            
            overrideBlackmothNailDamage = true;
            overrideBlackmothLore = true;
            overrideBlackmothCloak = true;
            
            handicapAllNonFireAttacks = false;

            nailmasterGloryCost = 3;
            fireballSoulAddOnHit = 11;
            fireballDamageBase = 8;
            fireballDamagePerNailLvl = 3;
            fireballMagmaDamageBase = 3;
            fireballMagmaDamagePerNailLvl = 2;
            laserDamageBase = 15;
            laserDamagePerNailLvl = 3;
            pillarDamageBase = 10;
            pillarDamagePerNailLvl = 6;
            pillarSecondaryDamageBase = 5;
            pillarSecondaryAttacks = 4;
            trailDamageBase = 5;
            trailDamagePerNailLvl = 4;
            trailSecondaryDamageBase = 0;
            trailSecondaryDamagePerNailLvl = 0;

            // Causes lasers to only fire when damage taken
            lasersWhenShieldBlocksAllDmg = false;

            useEnglishLoreWhenLanguageMissing = false;
            useEnglishWarningInfoWhenLanguageMissing = true;

            redwingFirstLaunch = true;

            

            fireballCooldownBase = 7f;
            laserCooldownBase = 10f;
            shieldCooldownBase = 30f;
            shieldCooldownReductionPerNailHit = 0.5f;
            
            settingsVersion = version_info.SETTINGS_VER;
        }
        public int settingsVersion { get => GetInt();
            private set => SetInt(value); }
        
        // Set to true on first launch and auto sets to false after. To give player warning info when they load into
        // a new game, and ruin Monomon's poem.
        public bool redwingFirstLaunch { get => GetBool(); set => SetBool(value); }
        
        // sets the nail damage to what it should be and not what blackmoth wants it to be.
        public bool overrideBlackmothNailDamage { get => GetBool(); private set => SetBool(value); }
        
        // Overrides any lore changed by blackmoth... with the exception of charm descriptions.
        public bool overrideBlackmothLore { get => GetBool(); private set => SetBool(value); }
        
        // Overrides the color of the cloak set by blackmoth.
        public bool overrideBlackmothCloak { get => GetBool(); private set => SetBool(value); }
        
        // Set to false ONLY if you have a non-blackmoth dashing mod.
        public bool useGreymothDashWhenBlackmothMissing { get => GetBool(); private set => SetBool(value); }
        
        // Set to true to make ALL non-redwing damage equal to 1. this includes spells. Although possibly not
        // fluke because bad programming.
        //
        // This is 100% non-canon as confirmed by team cherry. You are only making yourself weaker.
        // but if that's what it takes for you to have fun go for it.
        // you are a fire god and like any gods you can choose to intentionally hurt yourself.
        public bool handicapAllNonFireAttacks { get => GetBool(); private set => SetBool(value); }
        
        public bool useEnglishWarningInfoWhenLanguageMissing { get => GetBool(); private set => SetBool(value); }
        public bool useEnglishLoreWhenLanguageMissing { get => GetBool(); private set => SetBool(value); }
        
        
        public int nailmasterGloryCost { get => GetInt(); private set => SetInt(value); }
        
        public int fireballDamageBase { get => GetInt(); private set => SetInt(value); }
        public int fireballDamagePerNailLvl { get => GetInt(); private set => SetInt(value); }
        public int fireballMagmaDamageBase { get => GetInt(); private set => SetInt(value); }
        public int fireballMagmaDamagePerNailLvl { get => GetInt(); private set => SetInt(value); }
        public int fireballSoulAddOnHit { get => GetInt(); private set => SetInt(value); }
        public int laserDamageBase { get => GetInt(); private set => SetInt(value); }
        public int laserDamagePerNailLvl { get => GetInt(); private set => SetInt(value); }
        public int pillarDamageBase { get => GetInt(); private set => SetInt(value); }
        public int pillarDamagePerNailLvl { get => GetInt(); private set => SetInt(value); }
        public int pillarSecondaryDamageBase { get => GetInt(); private set => SetInt(value); }
        public int pillarSecondaryDamagePerNailLvl { get => GetInt(); private set => SetInt(value); }
        public int pillarSecondaryAttacks { get => GetInt(); private set => SetInt(value); }
        public int trailDamageBase { get => GetInt(); private set => SetInt(value); }
        public int trailDamagePerNailLvl { get => GetInt(); private set => SetInt(value); }
        public int trailSecondaryDamageBase { get => GetInt(); private set => SetInt(value); }
        public int trailSecondaryDamagePerNailLvl { get => GetInt(); private set => SetInt(value); }
        
        
        public bool lasersWhenShieldBlocksAllDmg { get => GetBool(); private set => SetBool(value); }
        
        public float fireballCooldownBase { get => GetFloat(); private set => SetFloat(value); }
        public float laserCooldownBase { get => GetFloat(); private set => SetFloat(value); }
        public float shieldCooldownBase { get => GetFloat(); private set => SetFloat(value); }
        public float shieldCooldownReductionPerNailHit { get => GetFloat(); private set => SetFloat(value); }
        
    }


    public class redwing_settings : IModSettings
    {
        // none needed

    }



}
