using Microsoft.VisualBasic;
using System;
using System.ComponentModel.DataAnnotations;

namespace BankManagement.Models
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }
        public double Amount { get; set; }
        public int Period { get; set; }
        public double ProcessingFee { get; set; }
        public double ReceivedAmmount { get; set; }
        public double TotalPayableAmount { get; set; }
        public double InterestRate { get; set; }
        public double MonthlyReturnable { get; set; }
        public string LoanType { get; set; }
        public double YearlyIncome { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; }
        public string Phone { get; set; }
        public int UserId { get; set; }

    }
}
