using DingoDataAccess.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess
{
    public class MessageHandler : IMessageHandler
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
                List<IMessageModel> messages = await GetMessages(RecipientId);

                // add the message
                if (messages.Contains(Message) is false)
                {
                    messages.Add(Message);
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
                return new();
            }

            try
            {
                // get the messages for the recipient
                List<IMessageModel> messages = new();

                var result = await db.ExecuteSingleProcedure<string, dynamic>(GetMessagesProcedure, new { Id });

                var tmp = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MessageModel>>(result);

                foreach (var item in tmp)
                {
                    messages.Add(item);
                }

                return messages;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to get messages for {SenderId} Error: {Error}", Id, e);
                return new();
            }
        }
    }
}
