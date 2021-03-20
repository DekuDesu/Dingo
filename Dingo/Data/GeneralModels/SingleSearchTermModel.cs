using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dingo.Data.GeneralModels
{
    public class SingleSearchTermModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(3, ErrorMessage = "Enter at least 1 character.")]
        [MaxLength(100, ErrorMessage = "Can't be more than 100 characters.")]
        public string Term { get; set; }
    }
}
