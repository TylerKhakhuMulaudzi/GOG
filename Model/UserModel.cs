using System.ComponentModel.DataAnnotations;

namespace APPR_Try1.Model
{
    public class UserModel
    {

            [Required(ErrorMessage = "Username is required")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Contact number is required")]
            [Phone(ErrorMessage = "Invalid phone number")]
            public string ContactNumber { get; set; }

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        
    }
}
