namespace ValdemoroEn1.Models
{
    public class AppLanguage(string culture, string language)
    {
        public string Culture { get; set; } = culture;
        public string Language { get; set; } = language;
    }
}
