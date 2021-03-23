using System;

namespace DingoDataAccess
{
    public interface IMessageModel
    {
        string Message { get; set; }
        string SenderId { get; set; }
        DateTime TimeSent { get; set; }
    }
}