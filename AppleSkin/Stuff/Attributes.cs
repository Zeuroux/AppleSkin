using AppleSkin.Extensions;
using OnixRuntime.Api;
using OnixRuntime.Api.Maths;
using OnixRuntime.Api.UI;
using System.Runtime.InteropServices;

namespace AppleSkin
{
    public struct HungerAttributes
    {
        public int Hunger;
        public float Saturation;
        public float Exhaustion;
        public bool HasHungerEffect;
        public ushort UpdateCounter;
    }

    public struct HealthAttributes
    {
        public int Health;
        public int HealthMax;
        public int Absorption;
        public bool IsHardcore;
        public bool IsFreezing;
        public bool HasRegenerationEffect;
        public bool HasWitherEffect;
        public bool HasPoisonEffect;
        public ushort UpdateCounter;
        public int AbsorptionOffset;
        public int BobIndex;
        public bool Changed;
        public int LastHealth;
        public int LastHealthMax;
        public int LastAbsorption;
    }
}
