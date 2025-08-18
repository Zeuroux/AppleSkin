namespace AppleSkin.Extensions
{
    static internal class FloatExtensions
    {
        public static float NextUp(this float f)
        {
            if (float.IsNaN(f) || f == float.PositiveInfinity)
                return f;
            int bits = BitConverter.SingleToInt32Bits(f);
            if (f >= 0)
                bits++;
            else
                bits--;
            return BitConverter.Int32BitsToSingle(bits);
        }
    }

}
