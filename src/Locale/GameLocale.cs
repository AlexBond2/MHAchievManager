using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHAchievManager.Locale
{
    public static class GameLocale
    {
        public record LocaleInfo(string Code, string DisplayName);

        // All supported game locales
        public static readonly LocaleInfo[] Locales =
        [
            new("en_us", "English"),
            new("ru_ru", "Русский (Russian)"),
            new("fr_fr", "Français (French)"),
            new("de_de", "Deutsch (German)"),
            new("es_mx", "Español (Spanish)"),
            new("pt_br", "Português (Portuguese)"),
            new("ko_kr", "한국어 (Korean)"),
            new("ja_jp", "日本語 (Japanese)"),
            new("zh_tw", "繁體中文 (Traditional Chinese)")
        ];

        public static readonly string DefaultLocale = "en_us";
    }
}
