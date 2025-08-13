namespace AppleSkin
{
    public class FoodItem
    {
        required public int Hunger { get; set; }
        required public float Saturation { get; set; }
        public bool AlwaysConsumable { get; set; } = false;
    }
}