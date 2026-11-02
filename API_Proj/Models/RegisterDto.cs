using System.ComponentModel.DataAnnotations;

namespace API_proj.Models
{
    public class RegisterDto
    {
        [Required, StringLength(50)]  
        public string UserName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string UserSurname { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, RegularExpression("^(Organiser|Participant)$",
            ErrorMessage = "Role must be Organiser or Participant.")]
        public string Role { get; set; } = "Participant";

        [StringLength(20)]
        public string? PhoneNumber { get; set; }
    }
}