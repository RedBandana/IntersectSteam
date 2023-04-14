using System.Linq;

namespace IntersectSteam
{
    public class Utility
    {
        public static string GetIso639Language(string language)
        {
            var iso639Language = Iso639.Language.FromName(language, true)
                        .Where(l => !string.IsNullOrEmpty(l.Part1) && l.Type == Iso639.LanguageType.Living)
                        .FirstOrDefault();

            if (iso639Language != null)
                return iso639Language.Part1;
            else
                return "en";
        }
    }
}
