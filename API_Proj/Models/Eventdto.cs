using System.ComponentModel.DataAnnotations;

namespace API_proj.Models
{
    public class EventDto
    {
     [StringLength(1000)]
        public string? Description { get; set; }

       [Required]
     public DateTime EventDate { get; set; }

     [Required, StringLength(200)]


     public string Location { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TargetAudience { get; set; }
    }
}