using EllieMae.Encompass.BusinessObjects.Loans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EncompassLoanApplication.ResponseObjects 
{
    public class LoanDetailResponse: BaseResponse
    {
        public string LoanId { get; set; }
        public string LoanName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string State { get; set; }
        public string LoanToValue { get; set; }
        public string InterestRate { get; set; }
        public string Term { get; set; }
        public string MonthlyPayment { get; set; }
        public int NumberOfAttachmentsSaved { get; set; }
    }

    public class LoanSchema: BaseResponse
    {
        public Loan loan { get; set; }
    }
    
    public class CreateLoanResponse:BaseResponse
    {
        public Guid LoanGuid { get; set; }
        public string LoanId { get; set; }
        public string LoanFolder { get; set;}
    }

    public class GetAttachmentListResponse: BaseResponse
    {
        public List<AttachmentsList> Attachments { get; set; }
        public int AttachementCount { get; set; }
        public string LoanId { get; set; }
        public string LoanName { get; set; }
        public string LoanNumber { get; set; }
    }
    public class AttachmentsList
    {
        public string AttachmentName { get; set; }
        public DateTime Date { get; set; }
    }

    public class AttachmentUrl
    {

    }
}