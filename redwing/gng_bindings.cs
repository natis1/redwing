namespace redwing
{
    public static class gng_bindings
    {
        public static bool applyBindings = false;
        public static bool applyNailBinding = false;
        public static bool applySpellBinding = false;
        public static bool applyCharmBinding = false;
        public static bool applyHealthBinding = false;


        public static bool hasNailBinding()
        {
            if (applyBindings && applyNailBinding)
            {
                return ( (PlayerData.instance.currentBossSequence.bindings & BossSequenceController.ChallengeBindings.Nail) != 0);
            }
            return false;
        }
        
        public static bool hasSpellBinding()
        {
            if (applyBindings && applySpellBinding)
            {
                return ( (PlayerData.instance.currentBossSequence.bindings & BossSequenceController.ChallengeBindings.Soul) != 0);
            }
            return false;
        }
        
        public static bool hasCharmBinding()
        {
            if (applyBindings && applyCharmBinding)
            {
                return ( (PlayerData.instance.currentBossSequence.bindings & BossSequenceController.ChallengeBindings.Charms) != 0);
            }
            return false;
        }
        
        public static bool hasHealthBinding()
        {
            if (applyBindings && applyHealthBinding)
            {
                return ( (PlayerData.instance.currentBossSequence.bindings & BossSequenceController.ChallengeBindings.Shell) != 0);
            }
            return false;
        }
    }
}