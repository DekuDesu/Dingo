using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DingoDataAccess;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;

namespace DingoDataAccess.Tests
{
    public class SqlDataAccessTests
    {
        [Fact]
        public void TestMessageModelSerialization()
        {
            //MessageModel x = new()
            //{
            //    SenderId = "34c9d2c5-3895-43c5-aacc-aabc026122da",
            //    Message = "Hey",
            //    TimeSent = DateTime.UtcNow
            //};

            //List<IMessageModel> s = new();
            //s.Add(x);

            //var a = Newtonsoft.Json.JsonConvert.SerializeObject(s);
            //List<MessageModel> b = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MessageModel>>(a);
        }
    }
}
