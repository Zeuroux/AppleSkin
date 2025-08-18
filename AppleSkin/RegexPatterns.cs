using System.Text.RegularExpressions;

namespace AppleSkin
{
    public static partial class RegexPatterns
    {
        [GeneratedRegex(@"#hover_text"":""([^""]+)""")]
        public static partial Regex HoverTextRegex();
        [GeneratedRegex(@"//.*")]
        public static partial Regex SingleLineCommentRegex();
        [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
        public static partial Regex MultiLineCommentRegex();

        [GeneratedRegex(@"""#container_item_modifier"":\s*([12])")]
        public static partial Regex ItemCategoryRegex();
    }
}
