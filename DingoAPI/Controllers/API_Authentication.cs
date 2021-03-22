using DingoAuthentication.Encryption;
using DingoDataAccess.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DingoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class API_Authentication : ControllerBase
    {
        private readonly ILogger<API_Authentication> logger;

        private readonly ISymmetricHandler<EncryptedDataModel> symmetricHandler;

        private readonly IOAuthHandler oauth;

        private readonly IPasswordHasher<dynamic> hasher;

        private readonly ISignatureHandler signatureHandler;

        private static Dictionary<string/*Id*/, List<string/*Authenticated Accounts*/>> AuthenticatedSessions = new();

        private static SemaphoreSlim Limiter = new(1, 1);

        public API_Authentication(ILogger<API_Authentication> logger, ISymmetricHandler<EncryptedDataModel> symmetricHandler, IOAuthHandler oauth, IPasswordHasher<dynamic> hasher, ISignatureHandler signatureHandler)
        {
            this.logger = logger;
            this.symmetricHandler = symmetricHandler;
            this.oauth = oauth;
            this.hasher = hasher;
            this.signatureHandler = signatureHandler;
        }

        [HttpPost]
        public async Task<ActionResult> Post(EncryptedMessageModel EncryptedMessage)
        {
            // attempt to get session using Id in encrypted message
            if (API_Sessions.TryGetSession(EncryptedMessage.Id, out IEncryptedSessionModel EncryptedSession))
            {
                // make sure that the data that was send to us is actually from the same person that the initial session was from
                if (signatureHandler.Verify(EncryptedMessage.EncryptedData.Data, EncryptedMessage.EncryptedData.Signature, EncryptedSession.X509IdentityKey))
                {
                    // now we know that it's the same person we can attempt to decrypt the message
                    if (symmetricHandler.TryDecrypt(EncryptedMessage.EncryptedData, EncryptedSession.AsymmetricKey, out string DecryptedMessage))
                    {
                        try
                        {
                            // since we were able to decrypt the message we should attempt to deserialize it into a authentication request
                            IAuthenticationRequestModel request = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthenticationRequestModel>(DecryptedMessage);

                            if (request is null)
                            {
                                logger.LogError("Deserialized Authentication Request was null");
                                return base.BadRequest("Failed to deserialize request into a valid Authentication Request.");
                            }

                            string dbOAuth = await oauth.GetOAuth(request.Id);

                            if (string.IsNullOrEmpty(dbOAuth))
                            {
                                return base.Unauthorized("Authentication Failed. User Id or OAuth is incorrect.");
                            }

                            var result = hasher.VerifyHashedPassword(new { }, dbOAuth, request.OAuth);

                            if (result is PasswordVerificationResult.Success)
                            {
                                // store which account the session Id is authorized to access
                                await StoreSession(EncryptedMessage.Id, request.Id);

                                return base.Ok();
                            }

                            return base.Unauthorized("Authentication Failed. User Id or OAuth is incorrect.");
                        }
                        catch (Exception e)
                        {
                            logger.LogError("Failed to deserialize authentication request {RawData}, {Error}", DecryptedMessage, e);

                            return base.BadRequest("Failed to deserialize request into a valid Authentication Request.");
                        }
                    }

                    logger.LogWarning("API_Authenticate POST, Failed to verify signature on message for {Id}", EncryptedMessage.Id);

                    // return Unauthorized when we failed to decrypt the message
                    return base.Unauthorized("Invalid or Expired session Id, use /API_Sessions to request a new one.");
                }

                logger.LogWarning("API_Authenticate POST, Failed to decrypt message for {Id}", EncryptedMessage.Id);

                // return Unauthorized when we failed to decrypt the message
                return base.Unauthorized("Invalid or Expired session Id, use /API_Sessions to request a new one.");
            }

            // return Unauthorized when no session was found
            return base.Unauthorized("Invalid or Expired session Id, use /API_Sessions to request a new one.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SessionId">
        ///     The Id provided by API_Sesssions
        /// </param>
        /// <param name="Id">
        ///     The Id of the Account that the session is authenticated for
        /// </param>
        /// <returns></returns>
        private async Task StoreSession(string SessionId, string Id)
        {
            // Dictionary layout
            /*
                AuthenticatedSessions (Dictionary)
                {
                    SessionId (Guid string)
                    {
                        Id (Guid String),
                        Id (Guid String),
                        Id (Guid String)
                    }
                } 

                where SessionId is the session Id given to the other party using API_Sessions
                where Id is the Account Id in the OAuth db
            */
            if (AuthenticatedSessions.ContainsKey(SessionId))
            {
                if (AuthenticatedSessions[SessionId].Contains(Id) is false)
                {
                    await Limiter.WaitAsync();

                    AuthenticatedSessions[SessionId].Add(Id);

                    Limiter.Release();
                }
            }
            else
            {
                await Limiter.WaitAsync();

                AuthenticatedSessions.Add(SessionId, new() { Id });

                Limiter.Release();
            }
        }

        /// <summary>
        /// Checks to see if the session Id Provided has access to the given account
        /// </summary>
        /// <param name="SessionId"></param>
        /// <param name="IdAttemptingToAccess"></param>
        /// <returns></returns>
        public static bool IsAuthenticated(string SessionId, string IdAttemptingToAccess)
        {
            if (AuthenticatedSessions.ContainsKey(SessionId))
            {
                if (AuthenticatedSessions[SessionId].Contains(IdAttemptingToAccess))
                {
                    return true;
                }
            }
            return false;
        }

        public static async Task RemoveSession(string SessionId)
        {
            await Limiter.WaitAsync();

            AuthenticatedSessions.Remove(SessionId);

            Serilog.Log.Information("Removed Authenticated Session {Id} Remaining({RemainingAuthenticatedSessions})", SessionId, AuthenticatedSessions.Count);

            Limiter.Release();
        }
    }
}
