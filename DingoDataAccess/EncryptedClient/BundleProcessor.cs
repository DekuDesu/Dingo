using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DingoAuthentication.Encryption;

namespace DingoDataAccess
{
    /// <summary>
    /// Creates secrets and handles Database interaction with bundles
    /// </summary>
    public class BundleProcessor<TKeyBundleType, TEncryptedDataModelType, TSignedKeyModelType> : IBundleProcessor
        where TSignedKeyModelType : ISignedKeyModel, new()
        where TKeyBundleType : IKeyBundleModel<TSignedKeyModelType>, new()
        where TEncryptedDataModelType : IEncryptedDataModel, new()
    {
        private readonly ILogger<BundleProcessor<TKeyBundleType, TEncryptedDataModelType, TSignedKeyModelType>> logger;

        private readonly IKeyAndBundleHandler<TKeyBundleType, TSignedKeyModelType> bundleHandler;

        private readonly IEncryptionClient<TEncryptedDataModelType, TSignedKeyModelType> encryptionClient;

        private readonly IEncryptedClientStateHandler clientStateHandler;

        public BundleProcessor(
                ILogger<BundleProcessor<TKeyBundleType, TEncryptedDataModelType, TSignedKeyModelType>> logger,
                IKeyAndBundleHandler<TKeyBundleType, TSignedKeyModelType> bundleHandler,
                IEncryptionClient<TEncryptedDataModelType, TSignedKeyModelType> encryptionClient,
                IEncryptedClientStateHandler clientStateHandler
            )
        {
            this.logger = logger;
            this.bundleHandler = bundleHandler;
            this.encryptionClient = encryptionClient;
            this.clientStateHandler = clientStateHandler;
        }

        /// <summary>
        /// Creates a bundle using a new encryption client and sends the bundle to the receipient's queue
        /// </summary>
        /// <param name="SenderId"></param>
        /// <param name="RecipientId"></param>
        /// <param name="encryptionClient"></param>
        /// <returns></returns>
        public async Task<bool> SendBundle(string SenderId, string RecipientId)
        {
            // get our keys
            var (X509IdentityKey, IdentityPrivateKey) = await bundleHandler.GetKeys(SenderId);

            if (X509IdentityKey != null && IdentityPrivateKey != null)
            {
                // generate a new bundle and save it to the server
                TKeyBundleType bundle = (TKeyBundleType)encryptionClient.GenerateBundle(X509IdentityKey, IdentityPrivateKey);

                // set the bundle in the recipients bundle dictionary
                if (await bundleHandler.SetBundle(RecipientId, SenderId, bundle))
                {
                    // save the state of our encryption client so we can create a secret later when we get the other persons bundle should they want to be friends
                    string clientState = encryptionClient.ExportState();

                    if (await clientStateHandler.SetState(SenderId, RecipientId, clientState))
                    {
                        logger.LogInformation("Successfully Created and sent bundle to {RecipientId} from {SenderId}", RecipientId, SenderId);
                        return true;
                    }
                    else
                    {
                        logger.LogError("Failed to set encryption client state for {Id}", SenderId);
                    }
                }
                else
                {
                    logger.LogError("Failed to set encryption client state for {Id}", SenderId);
                }
            }
            else
            {
                logger.LogError("Failed to {MethodName} for {Id} no keys retrieved from bundle handler", nameof(SendBundle), SenderId);
            }

            // if we got here we encountered and logged an error, return false.
            return false;
        }

        public async Task<bool> CreateSecretAndSendBundle(string SenderId, string RecipientId)
        {
            // check to see if we have a bundle waiting from the other party
            IKeyBundleModel<TSignedKeyModelType> bundle = await bundleHandler.GetBundle(SenderId, RecipientId);

            if (bundle != null)
            {
                // since the other person sent us a bundle we should generate our own and send it to them
                if (await SendBundle(SenderId, RecipientId))
                {
                    // since sendBundle generates new keys and saves the state of the encryption client on this object it should be assumed that we can create a secret immediately
                    string state = await clientStateHandler.GetState(SenderId, RecipientId);

                    encryptionClient.ImportState(state);

                    if (encryptionClient.CreateSecretUsingBundle(bundle))
                    {
                        logger.LogInformation("Created secret for {Id}, using {OtherId}'s bundle", SenderId, RecipientId);

                        // if we were able to sucessfully create a secret save our encryption state and remove the budle from our bundles
                        string clientState = encryptionClient.ExportState();

                        if (await clientStateHandler.SetState(SenderId, RecipientId, clientState))
                        {
                            logger.LogInformation("Set Client State for {Id}", SenderId);
                            // since we saved the state of our encryption client we can delete the bundle from the server
                            if (await bundleHandler.RemoveBundle(SenderId, RecipientId))
                            {
                                logger.LogInformation("Sucessfully created secret and removed bundle from server for {Id}", SenderId);
                                return true;
                            }
                            else
                            {
                                logger.LogError("Failed to remove bundle for friend {FriendId} for {Id}", RecipientId, SenderId);
                            }
                        }
                        else
                        {
                            logger.LogError("Failed to set encryption client state for {Id}", SenderId);
                        }
                    }
                    else
                    {
                        logger.LogError("Failed to create secret using Bundle for {Id}", SenderId);
                    }
                }
                else
                {
                    logger.LogError("Failed to generate bundle for other party {Id}", SenderId);
                }
            }
            else
            {
                logger.LogError("Failed to retrieve bundle for other party for {Id}", SenderId);
            }
            return false;
        }

        /// <summary>
        /// Checks to see if there is any waiting bundles and creates secrets for them
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<bool> CreateSecret(string Id, string OtherId)
        {
            try
            {
                // check to see if we have a bundle waiting for the OtherId
                TKeyBundleType bundle = await bundleHandler.GetBundle(Id, OtherId);

                // since we have a bundle waiting import our client state
                if (bundle != null)
                {
                    string state = await clientStateHandler.GetState(Id, OtherId);

                    if (state != null)
                    {
                        // since we have a state import it
                        encryptionClient.ImportState(state);

                        if (encryptionClient.CreatedSecret is false)
                        {
                            // create the secrey
                            if (encryptionClient.CreateSecretUsingBundle(bundle))
                            {
                                state = encryptionClient.ExportState();

                                if (await clientStateHandler.SetState(Id, OtherId, state))
                                {
                                    if (await bundleHandler.RemoveBundle(Id, OtherId))
                                    {
                                        logger.LogInformation("Succesfully created secret between {Id} and {OtherId}", Id, OtherId);
                                        return true;
                                    }
                                    {
                                        logger.LogError("Failed to remove bundle when creating secret for {Id}", Id);
                                    }
                                }
                                else
                                {
                                    logger.LogInformation("Failed to set the state of the encryption client for {Id}", Id);
                                }
                            }
                            else
                            {
                                logger.LogError("Failed to create secret using bundle for {Id}", Id);
                            }
                        }
                        else
                        {
                            logger.LogError("Attempted to create secret for client that already created secret");
                        }
                    }
                    else
                    {
                        logger.LogError("Failed to create secret for {Id}, no state found for {OtherId}", Id, OtherId);
                    }
                }
                else
                {
                    logger.LogError("Failed to retrieve bundle for {Id} other:{OtherId}", Id, OtherId);
                }
                return false;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to create all secrets for {Id} Error: {Error}", Id, e);
                return false;
            }
        }
    }
}
