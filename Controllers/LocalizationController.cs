using InventoryManagement.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Controllers
{
    public class LocalizationController : Controller
    {
        private readonly ILocalizationService _localizationService;

        public LocalizationController(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            _localizationService.SetLanguage(culture);
            
            // Also set the response cookie for the framework
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        public IActionResult SetTheme(string theme, string returnUrl)
        {
            _localizationService.SetTheme(theme);
            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public IActionResult GetCurrentSettings()
        {
            return Json(new 
            { 
                language = _localizationService.GetCurrentLanguage(),
                theme = _localizationService.GetCurrentTheme()
            });
        }
    }
}