using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ItransitionApp5.Models
{
    public class AccountController : Controller
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid && (await _userManager.FindByEmailAsync(model.Email)) == null)
            {
                User user = new User { Email = model.Email, UserName = model.Email, Name = model.Name, RegistrationDate = DateTime.Now, Status = Status.Active, UserRole = model.Admin ? UserRole.Admin : UserRole.User };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    switch (model.Admin)
                    {
                        case true:
                            await _userManager.AddToRoleAsync(user, "admin");
                            break;
                        default:
                            await _userManager.AddToRoleAsync(user, "user");
                            break;
                    }

                    await _signInManager.SignInAsync(user, true);
                    await UpdateLoginDate(model.Email);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    await UpdateLoginDate(model.Email);
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError("", "Blocked");
                }
            }
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> UpdateLoginDate(string name)
		{
            User user = await _userManager.FindByNameAsync(name);
            if(user != null)
			{
                user.LoginDate = DateTime.Now;
                await _userManager.UpdateAsync(user);
			}
            return new EmptyResult();
        }
    }
}
