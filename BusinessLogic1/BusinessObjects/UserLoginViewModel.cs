using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.BusinessObjects
{
    public class UserLoginViewModel
    {
        [Required(ErrorMessage = "Required")]
        //[StringLength(15, MinimumLength = 6, ErrorMessage = "Invalid min lenght is 6 max 15")]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
