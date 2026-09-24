using System.Text.RegularExpressions;
using Exiled.API.Features;

namespace AugatonLib.Text
{
    public static class SafeText
    {
        public const int DefaultMaxLength = 32;

        private static readonly Regex Forbidden = new Regex(
            @"[<>\\\p{Cc}\p{Cf}\p{Zl}\p{Zp}\u200B-\u200F\u202A-\u202E\u2060-\u206F\uFEFF]|[\uD800-\uDBFF](?![\uDC00-\uDFFF])|(?<![\uD800-\uDBFF])[\uDC00-\uDFFF]",
            RegexOptions.Compiled);

        public static string Sanitize(string value, int maxLength = DefaultMaxLength, string fallback = "?")
        {
            if (string.IsNullOrEmpty(value) || maxLength <= 0)
                return fallback;

            string cleaned = Forbidden.Replace(value, string.Empty).Trim();

            if (cleaned.Length == 0)
                return fallback;

            if (cleaned.Length <= maxLength)
                return cleaned;

            int cut = char.IsHighSurrogate(cleaned[maxLength - 1]) ? maxLength - 1 : maxLength;

            return cut == 0 ? fallback : cleaned.Substring(0, cut);
        }

        public static string Nickname(Player player, string fallback = "?")
        {
            return player is null ? fallback : Sanitize(player.Nickname, DefaultMaxLength, fallback);
        }
    }
}
