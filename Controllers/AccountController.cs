using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Models.ViewModel;

namespace TaskFlow.Controllers;

public class AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    : Controller
{
    [HttpGet]
    [Route("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email!);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Nieprawidłowy adres e-mail lub hasło.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user.UserName!, model.Password!, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        ModelState.AddModelError(string.Empty, "Nieprawidłowy adres e-mail lub hasło.");
        return View(model);
    }

    [HttpGet]
    [Route("register")]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [Route("register")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName ?? model.Email ?? "User"
        };
        var result = await userManager.CreateAsync(user, model.Password ?? string.Empty);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpPost]
    [Route("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await signInManager.SignOutAsync();
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    [HttpGet]
    [Route("account")]
    [Authorize]
    public async Task<IActionResult> Account()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction(nameof(Login));

        var model = new UserPanelModel
        {
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName
        };
        return View(model);
    }

    [HttpPost]
    [Route("account/change-password")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var user = await userManager.GetUserAsync(User);
            var panelModel = new UserPanelModel
            {
                Email = user?.Email ?? string.Empty,
                DisplayName = user?.DisplayName ?? string.Empty,
                ChangePasswordModel = model
            };
            return View("Account", panelModel);
        }

        var currentUser = await userManager.GetUserAsync(User);
        if (currentUser == null)
            return RedirectToAction(nameof(Login));

        var result = await userManager.ChangePasswordAsync(currentUser, model.CurrentPassword!, model.NewPassword!);
        if (result.Succeeded)
        {
            await signInManager.RefreshSignInAsync(currentUser);
            TempData["SuccessMessage"] = "Password.Changed.Success";
            return RedirectToAction(nameof(Account));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        var panelModelError = new UserPanelModel
        {
            Email = currentUser.Email ?? string.Empty,
            DisplayName = currentUser.DisplayName,
            ChangePasswordModel = model
        };
        return View("Account", panelModelError);
    }
}
