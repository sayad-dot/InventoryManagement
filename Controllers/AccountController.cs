using InventoryManagement.Models;
using InventoryManagement.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventoryManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // Registration View
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Registration Logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser 
                { 
                    UserName = model.Email, 
                    Email = model.Email,
                    FullName = model.FullName
                };
                
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Assign user role
                    await _userManager.AddToRoleAsync(user, "User");
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User created a new account with password.");
                    
                    return RedirectToAction("Index", "Home");
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // Login View
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Login Logic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return RedirectToLocal(returnUrl ?? "~/");
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToAction("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        // Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // Access Denied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Lockout
        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        // External Login Challenge
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        // External Login Callback - Handle both GET and POST requests
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }
            
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            
            if (result.IsLockedOut)
            {
                return RedirectToAction("Lockout");
            }
            else
            {
                // If the user does not have an account, get user info from provider
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var name = info.Principal.FindFirstValue(ClaimTypes.Name);
                
                // Debug logging to see what we're getting from the provider
                _logger.LogInformation("External login info: Provider={Provider}, Email={Email}, Name={Name}", 
                    info.LoginProvider, email ?? "NULL", name ?? "NULL");
                
                // Log all claims for debugging
                foreach (var claim in info.Principal.Claims)
                {
                    _logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
                }
                
                // Check if user already exists by email
                if (!string.IsNullOrEmpty(email))
                {
                    var existingUser = await _userManager.FindByEmailAsync(email);
                    if (existingUser != null)
                    {
                        // User exists, add this external login to their account
                        var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(existingUser, isPersistent: false);
                            _logger.LogInformation("Added external login {Provider} to existing user {Email}", info.LoginProvider, email);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            // If adding external login fails, it might already be linked - try to sign in
                            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                            if (signInResult.Succeeded)
                            {
                                return LocalRedirect(returnUrl);
                            }
                        }
                    }
                }

                // If we have both email and name, create the user automatically
                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(name))
                {
                    _logger.LogInformation("Attempting to create new user automatically: Email={Email}, Name={Name}", email, name);
                    
                    var newUser = new ApplicationUser 
                    { 
                        UserName = email, 
                        Email = email,
                        FullName = name,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    var createResult = await _userManager.CreateAsync(newUser);
                    if (createResult.Succeeded)
                    {
                        _logger.LogInformation("User created successfully: {Email}", email);
                        
                        // Assign user role
                        await _userManager.AddToRoleAsync(newUser, "User");
                        
                        // Add external login
                        var addLoginResult = await _userManager.AddLoginAsync(newUser, info);
                        if (addLoginResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(newUser, isPersistent: false);
                            _logger.LogInformation("User {Email} created and signed in using {Provider} provider.", email, info.LoginProvider);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            _logger.LogError("Failed to add external login for user {Email}", email);
                            foreach (var error in addLoginResult.Errors)
                            {
                                _logger.LogError("AddLogin Error: {Error}", error.Description);
                            }
                        }
                    }
                    else
                    {
                        // Log creation errors for debugging
                        _logger.LogError("Failed to create user {Email}", email);
                        foreach (var error in createResult.Errors)
                        {
                            _logger.LogError("CreateUser Error: {Error}", error.Description);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Missing required information - Email: {Email}, Name: {Name}", email ?? "NULL", name ?? "NULL");
                }
                
                // Only show confirmation form if we're missing required information or if auto-creation failed
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                
                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel
                {
                    Email = email ?? string.Empty,
                    FullName = name ?? string.Empty
                });
            }
        }

        // External Login Confirmation - GET
        [HttpGet]
        public async Task<IActionResult> ExternalLoginConfirmation(string? returnUrl = null)
        {
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;

            var model = new ExternalLoginConfirmationViewModel
            {
                Email = email ?? string.Empty,
                FullName = name ?? string.Empty
            };

            return View(model);
        }

        // External Login Confirmation - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    ModelState.AddModelError(string.Empty, "Error loading external login information during confirmation.");
                    return View(model);
                }

                // Check if user already exists by email
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    // Add login to existing user
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    if (addLoginResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        _logger.LogInformation("User {Email} added external login with {Name} provider.", model.Email, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        // If adding login fails, it might be because this external login is already associated with this user
                        // Try to sign them in directly
                        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
                        if (signInResult.Succeeded)
                        {
                            return LocalRedirect(returnUrl);
                        }
                        
                        foreach (var error in addLoginResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Create new user
                    var user = new ApplicationUser 
                    { 
                        UserName = model.Email, 
                        Email = model.Email,
                        FullName = model.FullName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        // Assign user role
                        await _userManager.AddToRoleAsync(user, "User");
                        
                        // Add external login
                        result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            _logger.LogInformation("User {Email} created an account using {Name} provider.", model.Email, info.LoginProvider);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = (await _signInManager.GetExternalLoginInfoAsync())?.LoginProvider;
            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}