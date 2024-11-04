using System.ComponentModel.DataAnnotations;

namespace APPR_Try1.Model
{
    public class VolunteerModels
    {
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please select an area of interest")]
        public string Interests { get; set; }

        public string Experience { get; set; }

        public string Availability { get; set; }

        public byte[] QualificationData { get; set; }
    }
}
