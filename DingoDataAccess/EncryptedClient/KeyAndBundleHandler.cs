using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DingoAuthentication.Encryption;
using Newtonsoft.Json;

namespace DingoDataAccess
{
    public class KeyAndBundleHandler<TKeyBundleType, TSignedKeyType> : IKeyAndBundleHandler<TKeyBundleType, TSignedKeyType> where TSignedKeyType : ISignedKeyModel, new() where TKeyBundleType : IKeyBundleModel<TSignedKeyType>, new()
    {
        private readonly ILogger<KeyAndBundleHandler<TKeyBundleType, TSignedKeyType>> logger;
        private readonly ISqlDataAccess db;

        private const string ConnectionStringName = "DingoMessagesConnection";

        private const string GetIdentityKeysProcedureName = "GetIdentityKeys";
        private const string SetIdentityKeysProcedureName = "SetIdentityKeys";

        private const string GetBundlesProcedureName = "GetBundles";
        private const string SetBundlesProcedureName = "SetBundles";

        public KeyAndBundleHandler(ILogger<KeyAndBundleHandler<TKeyBundleType, TSignedKeyType>> logger, ISqlDataAccess db)
        {
            this.logger = logger;
            this.db = db;
            this.db.ConnectionStringName = ConnectionStringName;
        }

        /// <summary>
        /// Removes and destroys the bundle, used normally when deleting an account or removing a friend from the friend list
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendId"></param>
        /// <returns></returns>
        public async Task<bool> RemoveBundle(string Id, string FriendId)
        {
            try
            {

                Dictionary<string, TKeyBundleType> bundles = await GetBundles(Id);

                if (bundles.ContainsKey(FriendId))
                {
                    bundles.Remove(FriendId);

                    await SetBundles(Id, bundles);
                }

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to remove bundle for {Id}, Error: {Error}", e);
                return false;
            }
        }

        /// <summary>
        /// Gets the bundle that allowss Id to communicate with Friend Id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendId"></param>
        /// <returns></returns>
        public async Task<TKeyBundleType> GetBundle(string Id, string FriendId)
        {
            try
            {

                Dictionary<string, TKeyBundleType> bundles = await GetBundles(Id);

                if (bundles.ContainsKey(FriendId))
                {
                    return bundles[FriendId];
                }

                return default;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get bundle for {Id}, Error: {Error}", e);
                return default;
            }
        }

        /// <summary>
        /// Sets the bundle that allowes Id to communicate with Friend Id
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="FriendId"></param>
        /// <param name="bundle"></param>
        /// <returns></returns>
        public async Task<bool> SetBundle(string Id, string FriendId, TKeyBundleType bundle)
        {
            try
            {

                Dictionary<string, TKeyBundleType> bundles = await GetBundles(Id);

                if (bundles.ContainsKey(FriendId))
                {
                    bundles[FriendId] = bundle;
                }
                else
                {
                    bundles.Add(FriendId, bundle);
                }

                await SetBundles(Id, bundles);

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to remove bundle for {Id}, Error: {Error}", e);
                return false;
            }
        }

        public async Task<(byte[] X509IdentityKey, byte[] IdentityPrivateKey)> GetKeys(string Id)
        {
            try
            {
                (string serializedX509IdentityKey, string serializedIdentityPrivateKey) = await db.ExecuteSingleProcedure<(string, string), dynamic>(GetIdentityKeysProcedureName, new { Id });

                //if (keys?.Count is null or < 2 or > 2)
                //{
                //    logger.LogError("Keys retreived from query do match expected count (2) actual ({Count}) Keys: {Keys} Keys[0]{Keys0}", keys?.Count, keys, keys?[0]);
                //    return default;
                //}

                byte[] X509IdentityKey = JsonConvert.DeserializeObject<byte[]>(serializedX509IdentityKey);
                byte[] IdentityPrivateKey = JsonConvert.DeserializeObject<byte[]>(serializedIdentityPrivateKey);

                return (X509IdentityKey, IdentityPrivateKey);
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set identity keys for {Id} Error: {Error}", Id, e);
                return default;
            }
        }

        public async Task<bool> SetKeys(string Id, byte[] X509IdentityKey, byte[] IdentityPrivateKey)
        {
            try
            {
                string serializedX509Identitykey = JsonConvert.SerializeObject(X509IdentityKey);
                string serializedIdentityPrivateKey = JsonConvert.SerializeObject(IdentityPrivateKey);

                await db.ExecuteVoidProcedure<dynamic>(SetIdentityKeysProcedureName, new { Id, X509IdentityKey = serializedX509Identitykey, PrivateIdentityKey = serializedIdentityPrivateKey });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to set identity keys for {Id} Error: {Error}", Id, e);
                return false;
            }
        }

        private async Task<bool> SetBundles(string Id, Dictionary<string, TKeyBundleType> bundles)
        {
            try
            {
                string serializedBundles = JsonConvert.SerializeObject(bundles);

                await db.ExecuteVoidProcedure<dynamic>(SetBundlesProcedureName, new { Id, Bundles = serializedBundles });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Error setting bundles for {Id} Error:{Error}", Id, e);
                return false;
            }
        }

        public async Task<Dictionary<string, TKeyBundleType>> GetBundles(string Id)
        {
            try
            {
                string serializedBundles = await db.ExecuteSingleProcedure<string, dynamic>(GetBundlesProcedureName, new { Id });

                Dictionary<string, TKeyBundleType> bundles = serializedBundles is null ? new() : JsonConvert.DeserializeObject<Dictionary<string, TKeyBundleType>>(serializedBundles);

                return bundles ?? new();
            }
            catch (Exception e)
            {
                logger.LogError("Error getting bundles for {Id} Error:{Error}", Id, e);
                return new();
            }
        }
    }
}
