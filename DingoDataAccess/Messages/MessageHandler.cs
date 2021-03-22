using DingoDataAccess.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public class MessageHandler
    {
        private readonly ISqlDataAccess db;
        private readonly ILogger<MessageHandler> logger;

        private const string ConnectionStringName = "DingoMessagesConnection";

        private const string SetMessagesProcedure = "SetMessages";
        private const string GetMessagesProcedure = "GetMessages";

        public MessageHandler(ISqlDataAccess _db, ILogger<MessageHandler> _logger)
        {
            db = _db;
            logger = _logger;
            db.ConnectionStringName = ConnectionStringName;
        }

        public async Task<bool> SendMessage(string SenderId, string RecipientId, IMessageModel Message)
        {
            if (Helpers.FullVerifyGuid(ref SenderId, logger) is false)
            {
                return false;
            }

            if (Helpers.FullVerifyGuid(ref RecipientId, logger) is false)
            {
                return false;
            }

            try
            {
                // serialize the message
                string message = Newtonsoft.Json.JsonConvert.SerializeObject(Message);

                // get the messages for the recipient
                List<string> messages;

                var result = await db.ExecuteSingleProcedure<string, dynamic>(GetMessagesProcedure, new { Id = RecipientId });

                messages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result);

                // add the message
                if (messages.Contains(message) is false)
                {
                    messages.Add(message);
                }

                // save the messages
                await db.ExecuteVoidProcedure<dynamic>(SetMessagesProcedure, new { Id = RecipientId, Messages = messages });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to send message From {SenderId} to {RecipientId} Error: {Error}", SenderId, RecipientId, e);
                return false;
            }
        }

        public async Task<List<IMessageModel>> GetMessages(string Id)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return null;
            }

            try
            {
                // get the messages for the recipient
                List<string> serializedMessages;

                var result = await db.ExecuteSingleProcedure<string, dynamic>(GetMessagesProcedure, new { Id });

                serializedMessages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(result);


                List<IMessageModel> messages = new();

                foreach (var item in serializedMessages)
                {
                    try
                    {
                        IMessageModel message = Newtonsoft.Json.JsonConvert.DeserializeObject<MessageModel>(item);
                        messages.Add(message);
                    }
                    catch
                    {
                        logger.LogWarning("Failed to deserialize message");
                    }
                }

                return messages;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get messages for {SenderId} Error: {Error}", Id, e);
                return null;
            }
        }
    }
}
