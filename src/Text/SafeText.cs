using System.Text.RegularExpressions;
using Exiled.API.Features;

namespace AugatonLib.Text
{
    public static class SafeText
    {
        public const int DefaultMaxLength = 32;

        private static readonly Regex Forbidden = new Regex(
            @"[<>\p{Cc}\p{Cf}\u200B-\u200F\u202A-\u202E\u2060-\u206F\uFEFF]",
            RegexOptions.Compiled);

        public static string Sanitize(string value, int maxLength = DefaultMaxLength, string fallback = "?")
        {
            if (string.IsNullOrEmpty(value))
                return fallback;

            string cleaned = Forbidden.Replace(value, string.Empty).Trim();

            if (cleaned.Length == 0)
                return fallback;

            return cleaned.Length <= maxLength ? cleaned : cleaned.Substring(0, maxLength);
        }

        public static string Nickname(Player player, string fallback = "?")
        {
            return player is null ? fallback : Sanitize(player.Nickname, DefaultMaxLength, fallback);
        }
    }
}
