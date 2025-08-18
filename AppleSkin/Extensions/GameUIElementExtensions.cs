using OnixRuntime.Api.Maths;
using OnixRuntime.Api.UI;

namespace AppleSkin.Extensions
{
    static internal class GameUIElementExtensions
    {
        public static Vec2 GetAbsolutePosition(this GameUIElement element)
        {
            Vec2 pos = default;
            for (var current = element; current != null; current = current.Parent)
                pos += current.Position;
            return pos;
        }
        public static bool FindMatchRecursive(this GameUIElement element, Func<GameUIElement, bool> what, Func<GameUIElement, bool> whenFound)
        {
            Stack<GameUIElement> stack = new();
            stack.Push(element);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (what(current) && !whenFound(current)) return false;
                if (current.Children is { Length: > 0 } children)
                    foreach (var child in children)
                        stack.Push(child);
            }
            return true;
        }
    }
}
