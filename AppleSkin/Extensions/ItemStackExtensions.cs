using OnixRuntime.Api.Items;

namespace AppleSkin.Extensions
{
    static internal class ItemStackExtensions
    {
        public static bool Matches(this ItemStack? stack, ItemStack? other)
        {
            if (stack is null || other is null)
                return false;

            if (stack.DisplayName != other.DisplayName ||
                stack.Item != other.Item ||
                stack.Aux != other.Aux ||
                stack.Count != other.Count)
                return false;

            if (stack.Enchants.Length != other.Enchants.Length)
                return false;

            return stack.Enchants.Length == other.Enchants.Length && stack.Enchants.All(e => other.Enchants.Any(oe => oe.Type == e.Type && oe.Level == e.Level));
        }
    }
}
