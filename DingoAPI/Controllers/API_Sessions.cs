using DingoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DingoAuthentication.Encryption;
using System.Threading;

namespace DingoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class API_Sessions : ControllerBase
    {
        private readonly ILogger<API_Authentication> logger;
        private readonly IDiffieHellmanRatchet dhRatchet;

        private static Dictionary<string, EncryptedSessionModel> Sessions { get; set; } = new();

        private SemaphoreSlim Limiter = new(1, 1);

        /// <summary>
        /// The default time that a session should remain in memory in ms. Default is 60 seconds
        /// </summary>
        public static int DefaultTimeout
        { get; set; } = 60_000;

        public API_Sessions(ILogger<API_Authentication> logger, IDiffieHellmanRatchet _dhRatchet)
        {
            this.logger = logger;
            dhRatchet = _dhRatchet;
        }

        [HttpGet]
        public async Task<ActionResult<ServerIdentityKeyModel>> Get()
        {
            await Helpers.Wait(100);
            return new ServerIdentityKeyModel() { X509IdentityKey = Startup.X509IdentityKey };
        }

        [HttpPost]
        public async Task<ActionResult<HandshakeMaterial>> Post(HandshakeMaterial handshakeMaterial)
        {
            if (handshakeMaterial is null)
            {
                logger.LogWarning("/EncryptedSessions POST param was null {ParamName}", nameof(handshakeMaterial));
                return NoContent();
            }

            if (handshakeMaterial.Signature?.Length is null or 0)
            {
                logger.LogWarning("/EncryptedSessions POST param was null {ParamName}", nameof(handshakeMaterial.Signature));
                return BadRequest("Signature missing");
            }

            if (handshakeMaterial.X509IdentityKey?.Length is null or 0)
            {
                logger.LogWarning("/EncryptedSessions POST param was null {ParamName}", nameof(handshakeMaterial.X509IdentityKey));
                return BadRequest("X509IdentityKey missing");
            }

            if (handshakeMaterial.PublicKey?.Length is null or 0)
            {
                logger.LogWarning("/EncryptedSessions POST param was null {ParamName}", nameof(handshakeMaterial.PublicKey));
                return BadRequest("Public Key missing");
            }

            await Helpers.Wait(100);

            // reset the ratchet keys to create a new secret
            dhRatchet.GenerateBaseKeys();

            // attempt to merge the two keys to create a secret
            if (dhRatchet.TryCreateSharedSecret(handshakeMaterial.X509IdentityKey, handshakeMaterial.PublicKey, handshakeMaterial.Signature))
            {
                // try to sign the new ratchet public key with the servers static key so the other party can verify no MIM is giving them
                // an incorrect key
                if (dhRatchet.TrySignKey(dhRatchet.PublicKey, Startup.PrivateIdentityKey, out byte[] Signature))
                {
                    string Id = Guid.NewGuid().ToString();

                    // create and store a 256 bit key that can be used for asymmetric encryption or key deriviation
                    EncryptedSessionModel newSession = new()
                    {
                        AsymmetricKey = dhRatchet.PrivateKey,
                        X509IdentityKey = handshakeMaterial.X509IdentityKey
                    };

                    // store the session
                    await StoreSession(Id, newSession);

                    // return the session ID to the other party and include our ratchet information so they can create a secret with us as well
                    HandshakeMaterial result = new()
                    {
                        PublicKey = dhRatchet.PublicKey,
                        Signature = Signature,
                        X509IdentityKey = Startup.X509IdentityKey,
                        Id = Id
                    };


                    return result;
                }

                logger.LogWarning("Failed to sign key during handshake.");

                return base.Problem("Server Failed To Create Handshake =(");
            }

            logger.LogWarning("Failed to create shared secret.");

            return base.Problem("Handshake Failed");
        }

        /// <summary>
        /// Attempts to retrieve the Id's session from memory
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="encryptedSession"></param>
        /// <returns></returns>
        public static bool TryGetSession(string Id, out IEncryptedSessionModel encryptedSession)
        {
            if (Sessions.ContainsKey(Id))
            {
                encryptedSession = Sessions[Id];
                return true;
            }

            encryptedSession = default;

            return false;
        }

        private async Task StoreSession(string Id, EncryptedSessionModel session)
        {
            await Limiter.WaitAsync();

            // store the session
            Sessions.Add(Id, session);

            // make it so sessions expire automatically

            System.Timers.Timer timer = new()
            {
                Interval = DefaultTimeout,
                AutoReset = false
            };

            timer.Elapsed += async (x, y) =>
                {
                    Sessions.Remove(Id);
                    timer?.Close();
                    timer?.Dispose();
                    logger.LogInformation("Session {SessionId} expired. Total({DictionaryCount})", Id, Sessions.Count);
                    await API_Authentication.RemoveSession(Id);
                };

            timer.Start();

            Limiter.Release();
        }
    }
}
