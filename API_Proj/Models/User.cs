using System.ComponentModel.DataAnnotations;

namespace API_proj.Models
{
    public class User
    {
      [Key]
    public int UserID { get; set; }

    [Required, StringLength(50)]
   public string UserName { get; set; } = string.Empty;

      [Required, StringLength(50)]

     public string UserSurname { get; set; } = string.Empty;

        [Required, StringLength(100)]

        public string Email { get; set; } = string.Empty; //new email field : Excluded from Part 1 ERD


     [Required, StringLength(255)]
     public string PasswordHash { get; set; } = string.Empty;

      [Required, StringLength(20)]
       public string Role { get; set; } = "Participant";

      [StringLength(20)]
      public string? PhoneNumber { get; set; }



    }
}