using System;

namespace DingoDataAccess.Models
{
    public interface IMessageModel
    {
        string Message { get; set; }
        string SenderId { get; set; }
        DateTime TimeSent { get; set; }
    }
}