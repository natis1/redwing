using Modding;

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
                return BossSequenceController.BoundNail;
            }
            return false;
        }
        
        public static bool hasSpellBinding()
        {
            if (applyBindings && applySpellBinding)
            {
                return BossSequenceController.BoundSoul;
            }
            return false;
        }
        
        public static bool hasCharmBinding()
        {
            if (applyBindings && applyCharmBinding)
            {
                return BossSequenceController.BoundCharms;
            }
            return false;
        }
        
        public static bool hasHealthBinding()
        {
            if (applyBindings && applyHealthBinding)
            {
                return BossSequenceController.BoundShell;
            }
            return false;
        }
    }
}