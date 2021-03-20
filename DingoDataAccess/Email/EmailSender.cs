using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DingoDataAccess.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> logger;

        public EmailSender(ILogger<EmailSender> _Logger)
        {
            logger = _Logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            logger.LogWarning("Simulated sending confirmation email");
            await Task.Run(() => Thread.Sleep(10));
        }
    }
}
