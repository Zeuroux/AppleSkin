namespace AppleSkin
{
    public class Effect     {
        public string Name { get; set; }
        public float Chance { get; set; }
        public int Duration { get; set; }
        public int Amplifier { get; set; }
    }
    public class FoodItem
    {
        required public int Hunger { get; set; }
        required public float Saturation { get; set; }
        public bool AlwaysConsumable { get; set; } = false;
        public Effect[] Effects { get; set; } = [];
    }
}