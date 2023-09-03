using System.ComponentModel.DataAnnotations;
using System;

namespace BankManagement.Models
{
    public class FixedDeposit
    {
        [Key]
        public int Id { get; set; }
        public string DebitAcc { get; set; }
        public string FDAccNo { get; set; }
        public int InvestAmount { get; set; }
        public int Period { get; set; }
        public string InterestPayout { get; set; }
        public double InterestRate { get; set; }
        public DateTime MaturityDate { get; set; }
        public int MaturityAmount { get; set; }
        public string NomineeName { get; set; }
        public DateTime DOB { get; set; }
        public int UserId { get; set; }
    }
}
