using System.ComponentModel.DataAnnotations;

namespace BankManagement.ViewModel
{
    public class ElectricityBillVM
    {
        public int Id { get; set; }
        public string Provider { get; set; }
        public string CustomerNumber { get; set; }
        public int Amount { get; set; }
        public string BankAccNo { get; set; }
    }
}
