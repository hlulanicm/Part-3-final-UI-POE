using System.ComponentModel.DataAnnotations;

namespace API_proj.Models
{
    public class Results
    {
        [Key]
        public int ResultID { get; set; }

        public int EnrollmentID { get; set; }

        public int? Position { get; set; }

        public TimeSpan? FinishTime { get; set; }

        public TimeSpan? StartTime { get; set; }
    }
}