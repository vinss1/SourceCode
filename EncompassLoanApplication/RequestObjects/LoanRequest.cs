using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EncompassLoanApplication.RequestObjects
{
    public class CreateLoanRequest
    {
        public string LoanName { get; set; }
        public string BorrowerFirstName { get; set; }
        public string BorrowerLastName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public int ZipCode { get; set; }
        public string LoanToValue { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public decimal MonthlyPayment { get; set; }

    }
}