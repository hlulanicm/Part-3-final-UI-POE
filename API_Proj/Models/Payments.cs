using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_proj.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal Amount { get; set; }

        [Required, StringLength(30)]
        public string PaymentMethod { get; set; } = "Card";

        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}