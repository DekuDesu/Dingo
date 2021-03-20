using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dingo.Data.FriendList
{
    public class FriendSearchModel
    {
        [Required]
        [MinLength(1)]
        [MaxLength(256)]
        public string Name { get; set; }
    }
}
