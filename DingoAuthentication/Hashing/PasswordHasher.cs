using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using static BCrypt.Net.BCrypt;
using Microsoft.Extensions.Logging;

namespace DingoAuthentication
{
    public class PasswordHasher<TUser> : IPasswordHasher<TUser> where TUser : class
    {
        private readonly ILogger logger;

        public PasswordHasher(ILogger<PasswordHasher<TUser>> _logger)
        {
            logger = _logger;
        }

        public string HashPassword(TUser user, string password)
        {
            logger.LogInformation($"BCrypt was used to Hash incoming password.");
            return EnhancedHashPassword(password, HashType.SHA384);
        }

        public PasswordVerificationResult VerifyHashedPassword(TUser user, string hashedPassword, string providedPassword)
        {
            logger.LogInformation("BCrypt was used to verify incoming password");
            bool result = EnhancedVerify(providedPassword, hashedPassword, HashType.SHA384);
            return result ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}
