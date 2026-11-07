using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class LoginVm
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }

    public class RegisterVm
    {
        [Required, StringLength(50)]
        [Display(Name = "First name")]
        public string UserName { get; set; } = "";

        [Required, StringLength(50)]
        [Display(Name = "Surname")]
        public string UserSurname { get; set; } = "";

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = "";

        [StringLength(20)]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Required, MinLength(8, ErrorMessage = "The password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";

        [Required, RegularExpression("^(Organiser|Participant)$", ErrorMessage = "Role must be Organiser or Participant.")]
        public string Role { get; set; } = "Participant";
    }

    public class ProfileVm
    {
        [Required, StringLength(50)]
        [Display(Name = "First name")]
        public string UserName { get; set; } = "";

        [Required, StringLength(50)]
        [Display(Name = "Surname")]
        public string UserSurname { get; set; } = "";

        public string Email { get; set; } = "";

        [StringLength(20)]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Profile picture")]
        public IFormFile? Picture { get; set; }
    }
}
