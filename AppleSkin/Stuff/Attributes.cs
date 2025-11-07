namespace AppleSkin
{
    public struct HungerAttributes
    {
        public int Hunger;
        public float Saturation;
        public float Exhaustion;
        public bool HasHungerEffect;
        public ushort UpdateCounter;
        public bool ShouldJitter;
    }

    public struct HealthAttributes
    {
        public int Health;
        public int HealthMax;
        public int HealthOffset;
        public int Absorption;
        public int AbsorptionMax;
        public int AbsorptionOffset;
        public bool IsHardcore;
        public bool IsFreezing;
        public bool HasRegenerationEffect;
        public bool HasWitherEffect;
        public bool HasPoisonEffect;
        public bool ShouldJitter;
        public int BobIndex;
        public bool Changed;
        public int LastHealth;
        public int LastHealthMax;
        public int LastAbsorption;
    }

    public struct ArmorAttributes //why
    {
        public Dictionary<string, int> Armors = [];

        public ArmorAttributes() { }
    }
}
