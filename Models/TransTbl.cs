﻿using System;
using System.ComponentModel.DataAnnotations;

namespace BankManagement.Models
{
    public class TransTbl
    {
        [Key]
        public int TransID { get; set; }
        public int TransTypeID { get; set; }
        public int FromBankAccID { get; set; }
        public int ToBankAccID { get; set; }

        public UsersBankAccTbl FromBankAcc { get; set; } // Navigation property for sender bank account
        public UsersBankAccTbl ToBankAcc { get; set; }   // Navigation property for receiver bank account
        public TransTypeTbl TransType { get; set; }

        public string FromBankAccNumber { get; set; }
        public string ToBankAccNumber { get; set; }


        // Rest of the properties
        public string TransExtType { get; set; }
        public string MobileNumber { get; set; }
        public string Oparator { get; set; }
        public string OparatorType { get; set; }

        public string TransReason { get; set; }
        public float TransChargeAmount { get; set; }
        public float TransAmount { get; set; }
        public float TransTotalAmount { get; set; }
        public int StatusID { get; set; }
        public DateTime? CreatedAT { get; set; }
        public DateTime? UpdatedAT { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int Sender { get; set; }
        public int Receiver { get; set; }
        public int UserID { get; set; }
        public string Type { get; set; }
    }
}
