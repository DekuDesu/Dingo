using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DingoDataAccess.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;


namespace Dingo.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IStatusHandler statusHandler;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, IStatusHandler statusHandler)
        {
            _signInManager = signInManager;
            _logger = logger;
            this.statusHandler = statusHandler;
        }

        public async Task OnGet()
        {
            await statusHandler.SetStatus(User?.Claims?.FirstOrDefault()?.Value, DingoDataAccess.Enums.OnlineStatus.Offline);

            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out. {Method}", "GET");
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation("User logged out. {Method}", "POST");

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage();
            }
        }
    }
}
