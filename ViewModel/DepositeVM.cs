using System;

namespace BankManagement.ViewModel
{
    public class DepositeVM
    {
        public int Id { get; set; }
        public string AccountNo { get; set; }
        public int Amount { get; set; }
        public DateTime DepositeDate { get; set; }
        public int UserId { get; set; }

    }
}
