using ItransitionApp5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ItransitionApp5.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _applicationContext;

        public HomeController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<HomeController> logger, ApplicationContext applicationContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger; 
            _applicationContext = applicationContext;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            List<IndexViewModel> indexView = new List<IndexViewModel>();
            foreach (var user in _userManager.Users)
            {
                indexView.Add(new IndexViewModel
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    RegistrationDate = user.RegistrationDate,
                    LastLoginDate = user.LoginDate,
                    Role = user.UserRole,
                    Status = user.Status
                });
            }

            return View(indexView);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Messages()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
            List<MessagesView> userMessages = new List<MessagesView>();
            List<Message> messages = _applicationContext.Messages.Where(x => x.ReceiverId == currentUser.Id).Include(f => f.Sender).OrderBy(o => o.Id).ToList();
            
            foreach (var m in messages)
            {
                userMessages.Add(new MessagesView
                {
                    Sender = m.Sender,
                    MessageText = m.MessageText
                });
            }

            return View(userMessages);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(List<string> ids)
		{
            if(ids.Count != 0)
			{
                foreach (var id in ids)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);

                        await _userManager.DeleteAsync(user);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> BlockUser(List<string> ids)
        {
            if (ids.Count != 0)
            {
                foreach (var id in ids)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null && user.Status != Status.Block)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);

                        user.LockoutEnd = DateTime.Now + TimeSpan.FromMinutes(6000);
                        user.Status = Status.Block;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UnblockUser(List<string> ids)
        {
            if (ids.Count != 0)
            {
                foreach (var id in ids)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null && user.Status != Status.Active)
                    {
                        await _userManager.UpdateSecurityStampAsync(user);

                        user.LockoutEnd = null;
                        user.Status = Status.Active;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpgradeToAdmin(List<string> ids)
        {
            if (ids.Count != 0)
            {
                foreach (var id in ids)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null && user.UserRole != UserRole.Admin)
                    {
                        await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                        await _userManager.AddToRoleAsync(user, "admin");
                        user.UserRole = UserRole.Admin;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DowngradeToUser(List<string> ids)
        {
            if (ids.Count != 0)
            {
                foreach (var id in ids)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null && user.UserRole != UserRole.User)
                    {
                        await _userManager.RemoveFromRolesAsync(user, await _userManager.GetRolesAsync(user));
                        await _userManager.AddToRoleAsync(user, "user");
                        user.UserRole = UserRole.User;
                        await _userManager.UpdateAsync(user);
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendMessage(List<string> ids, string messageText)
        {
            if (ids.Count != 0)
            {
                foreach (var id in ids)
                {
                    User user = await _userManager.FindByIdAsync(id);
                    if (user != null && messageText != "")
                    {
                        var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                        var message = new Message() { SenderId = currentUser.Id, ReceiverId = user.Id, MessageText = messageText };

                        await _applicationContext.AddAsync(message);
                        await _applicationContext.SaveChangesAsync();
                    }
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}