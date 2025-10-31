using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace InventoryManagement.Services
{
    public interface ILocalizationService
    {
        void SetLanguage(string culture);
        void SetTheme(string theme);
        string GetCurrentLanguage();
        string GetCurrentTheme();
    }

    public class LocalizationService : ILocalizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LocalizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetLanguage(string culture)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            if (response != null)
            {
                response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }
        }

        public void SetTheme(string theme)
        {
            var response = _httpContextAccessor.HttpContext?.Response;
            if (response != null)
            {
                response.Cookies.Append(
                    "theme",
                    theme,
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }
        }

        public string GetCurrentLanguage()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies[CookieRequestCultureProvider.DefaultCookieName]?.Split('|')[0]?.Split('=')[1] ?? "en";
        }

        public string GetCurrentTheme()
        {
            return _httpContextAccessor.HttpContext?.Request.Cookies["theme"] ?? "light";
        }
    }
}