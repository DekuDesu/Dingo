using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DingoDataAccess.Models.Friends
{
    public class FullDisplayNameModel : IFullDisplayNameModel
    {
        public string Id { get; set; }
        public short UniqueIdentifier { get; set; }
    }
}
