using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_proj.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        public int OrganiserID { get; set; }

        public int? CategoryID { get; set; }


           [StringLength(1000)]
        public string? Description { get; set; }

        [Column(TypeName = "date")]
            public DateTime EventDate { get; set; }

        [Required, StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TargetAudience { get; set; }
    }
}