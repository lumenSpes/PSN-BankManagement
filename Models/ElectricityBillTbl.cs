using System.ComponentModel.DataAnnotations;

namespace BankManagement.Models
{
    public class ElectricityBillTbl
    {
        [Key]
        public int Id { get; set; }
        public string Provider { get; set; }
        [Required]
        [StringLength(8)]
        public string CustomerNumber { get; set; }
        [Required]
        public int Amount { get; set; }
        [Required]
        public string BankAccNo { get; set; }

    }
}
