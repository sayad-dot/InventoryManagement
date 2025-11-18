using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;
using InventoryManagement.Services;

namespace InventoryManagement.Controllers;

[Authorize]
public class SalesforceController : Controller
{
    private readonly ISalesforceService _salesforceService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<SalesforceController> _logger;

    public SalesforceController(
        ISalesforceService salesforceService,
        UserManager<ApplicationUser> userManager,
        ILogger<SalesforceController> logger)
    {
        _salesforceService = salesforceService;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Authorize()
    {
        try
        {
            // Use http for localhost, https for production
            var scheme = Request.Host.Host == "localhost" ? "http" : "https";
            var redirectUri = Url.Action("Callback", "Salesforce", null, scheme);
            var state = Guid.NewGuid().ToString();
            
            // Generate PKCE code verifier and challenge
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            
            _logger.LogInformation("Salesforce OAuth - Starting authorization flow with PKCE");
            _logger.LogInformation("Salesforce OAuth - Redirect URI: {RedirectUri}", redirectUri);
            
            // Store state and code verifier in session for validation
            HttpContext.Session.SetString("SalesforceOAuthState", state);
            HttpContext.Session.SetString("SalesforceCodeVerifier", codeVerifier);
            
            var authUrl = _salesforceService.GetAuthorizationUrl(redirectUri!, state, codeChallenge);
            _logger.LogInformation("Salesforce OAuth - Auth URL: {AuthUrl}", authUrl);
            
            return Redirect(authUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Authorize action");
            TempData["Error"] = $"Authorization error: {ex.Message}";
            return RedirectToAction("Index", "Profile");
        }
    }
    
    private string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
    
    private string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var challengeBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(codeVerifier));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    [HttpGet]
    public async Task<IActionResult> Callback(string code, string state)
    {
        if (string.IsNullOrEmpty(code))
        {
            TempData["Error"] = "Authorization failed. No code received from Salesforce.";
            return RedirectToAction("Index", "Profile");
        }

        // Validate state
        var savedState = HttpContext.Session.GetString("SalesforceOAuthState");
        if (state != savedState)
        {
            TempData["Error"] = "Invalid state parameter. Possible CSRF attack.";
            return RedirectToAction("Index", "Profile");
        }

        try
        {
            // Use http for localhost, https for production
            var scheme = Request.Host.Host == "localhost" ? "http" : "https";
            var redirectUri = Url.Action("Callback", "Salesforce", null, scheme);
            
            // Retrieve code verifier from session
            var codeVerifier = HttpContext.Session.GetString("SalesforceCodeVerifier");
            if (string.IsNullOrEmpty(codeVerifier))
            {
                TempData["Error"] = "Code verifier not found. Please try authorizing again.";
                return RedirectToAction("Index", "Profile");
            }
            
            _logger.LogInformation("Salesforce OAuth Callback - Redirect URI: {RedirectUri}", redirectUri);
            
            var accessToken = await _salesforceService.ExchangeCodeForTokenAsync(code, redirectUri!, codeVerifier);
            
            // Store access token in session for subsequent use
            HttpContext.Session.SetString("SalesforceAccessToken", accessToken);
            
            TempData["Success"] = "Successfully connected to Salesforce! You can now export your profile.";
            return RedirectToAction("ExportForm");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Salesforce OAuth callback");
            TempData["Error"] = $"Failed to connect to Salesforce: {ex.Message}";
            return RedirectToAction("Index", "Profile");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportForm()
    {
        var accessToken = HttpContext.Session.GetString("SalesforceAccessToken");
        if (string.IsNullOrEmpty(accessToken))
        {
            TempData["Info"] = "Please authorize Salesforce access first.";
            return RedirectToAction("Authorize");
        }

        var user = await _userManager.GetUserAsync(User);
        ViewBag.UserEmail = user?.Email;
        ViewBag.UserName = user?.FullName ?? user?.Email;
        
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Export(string companyName, string phone, string industry, string jobTitle)
    {
        var accessToken = HttpContext.Session.GetString("SalesforceAccessToken");
        if (string.IsNullOrEmpty(accessToken))
        {
            return Json(new { success = false, message = "Salesforce access token not found. Please authorize again." });
        }

        if (string.IsNullOrWhiteSpace(companyName))
        {
            return Json(new { success = false, message = "Company name is required." });
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Create Account (Company)
            var accountId = await _salesforceService.CreateAccountAsync(
                accessToken,
                companyName,
                phone ?? "",
                industry ?? "Other"
            );

            // Split user's full name into first and last name
            var fullName = user.FullName ?? user.Email ?? "User";
            var nameParts = fullName.Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : firstName;

            // Create Contact (Person)
            var contactId = await _salesforceService.CreateContactAsync(
                accessToken,
                accountId,
                firstName,
                lastName,
                user.Email!,
                jobTitle ?? "User"
            );

            _logger.LogInformation("Successfully exported to Salesforce - Account: {AccountId}, Contact: {ContactId}", accountId, contactId);

            return Json(new
            {
                success = true,
                message = "Successfully exported to Salesforce!",
                accountId = accountId,
                contactId = contactId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to Salesforce");
            return Json(new { success = false, message = $"Export failed: {ex.Message}" });
        }
    }
}
