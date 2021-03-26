using DingoDataAccess.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
                // get the serialized messages list
                var result = await db.ExecuteSingleProcedure<string, dynamic>(GetMessagesProcedure, new { Id = RecipientId });

                // if the list is null just create a new one
                List<MessageModel> messages = result is null ? new() : Newtonsoft.Json.JsonConvert.DeserializeObject<List<MessageModel>>(result);

                // add it to the list
                if (Message is MessageModel mm)
                {
                    // add the message
                    if (messages.Contains(mm) is false)
                    {
                        messages.Add(mm);
                    }
                }

                // serialize the list for storage
                result = Newtonsoft.Json.JsonConvert.SerializeObject(messages);

                // save the messages
                await db.ExecuteVoidProcedure<dynamic>(SetMessagesProcedure, new { Id = RecipientId, Messages = result });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to send message From {SenderId} to {RecipientId} Error: {Error}", SenderId, RecipientId, e);
                return false;
            }
        }

        public async Task<List<IMessageModel>> GetMessages(string Id, string IdToRetrieve)
        {
            if (Helpers.FullVerifyGuid(ref Id, logger) is false)
            {
                return new();
            }

            if (Helpers.FullVerifyGuid(ref IdToRetrieve, logger) is false)
            {
                return new();
            }

            try
            {
                List<MessageModel> messages = await GetMessages<List<MessageModel>>(Id);

                if (messages?.Count is null or 0)
                {
                    return new();
                }

                List<IMessageModel> result = new();

                var tmp = messages.Where(x => x.SenderId == IdToRetrieve).ToArray();

                if (tmp?.Length is null or 0)
                {
                    return new();
                }

                foreach (var item in tmp)
                {
                    result.Add(item);
                    messages.Remove(item);
                }

                await SetMessages(Id, messages);

                return result;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to Get Messages from {Id} for {IdToRetrieve} {Error}", Id, IdToRetrieve, e);

                return new();
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

                var rawMessages = await GetMessages<List<MessageModel>>(Id);

                foreach (var item in rawMessages)
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

        private async Task<T> GetMessages<T>(string Id)
        {
            try
            {
                string serializedMessages = await db.ExecuteSingleProcedure<string, dynamic>(GetMessagesProcedure, new { Id });

                return JsonConvert.DeserializeObject<T>(serializedMessages);
            }
            catch (Exception e)
            {
                logger.LogError("Failed to save messages for {Id} {Error}", Id, e);

                return default;
            }
        }
        private async Task<bool> SetMessages<T>(string Id, T Messages) where T : new()
        {
            try
            {
                string serialized = JsonConvert.SerializeObject(Messages);

                await db.ExecuteVoidProcedure<dynamic>(SetMessagesProcedure, new { Id, Messages = serialized });

                return true;
            }
            catch (Exception e)
            {
                logger.LogError("Failed to save messages for {Id} {Error}", Id, e);
                return false;
            }
        }
    }
}
