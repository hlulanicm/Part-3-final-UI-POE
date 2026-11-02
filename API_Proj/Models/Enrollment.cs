using System.ComponentModel.DataAnnotations;

namespace API_proj.Models
{
    public class Enrollment
    {
        [Key]
      public int EnrollmentID { get; set; }

      public DateTime EnrollmentDate { get; set; } = DateTime.Now;

       public int ParticipantID { get; set; }


       public int CategoryID { get; set; }

        [Required, StringLength(20)]


        public string Status { get; set; } = "Active";


       [StringLength(50)]
        public string? TargetAudience { get; set; }
    }
}