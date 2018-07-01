using System;
using System.Web.Http;
using System.Configuration;
using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.Client;
using System.IO;
using EllieMae.Encompass.BusinessObjects;
using System.Text;
using EncompassLoanApplication.ResponseObjects;
using EncompassLoanApplication.RequestObjects;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace EncompassLoanApplication.Controllers
{
    public class AccountController : ApiController
    {
        string loginName = ConfigurationManager.AppSettings.Get("UserName");
        string password = ConfigurationManager.AppSettings.Get("Password");
        string server = ConfigurationManager.AppSettings.Get("Server");
        EllieMae.Encompass.Client.Session s = null;
        StringBuilder sb = new StringBuilder();
        public Session login()
        {
            s = new Session();
            s.Start(server, loginName, password);
            return s;
        }

        [HttpGet]
        [Route("api/GetLoan")]
        public LoanDetailResponse GetLoanByLoanID(string loanId)
        {
            LoanDetailResponse ld = new LoanDetailResponse();
          // Loan loan = new Loan()


            string id = "{" + loanId + "}";
            try
            {                // Start the session
                login();
                
                sb.Append("log something: Login Success");
                // flush every 20 seconds as you do it               
                
                System.Web.Security.FormsAuthentication.SetAuthCookie(loginName, false);

                //Loan loan = s.Loans.Open("{fa8148a4-a482-4fa8-a6e6-ebbb3ae8e1cc}");
                Loan loan = s.Loans.Open(id);
               
                sb.AppendLine("log something:Loan Name: "+ loan.LoanName);
                GetAttachmentbyId(loan, ld);
                ld = generateDataTable(id, ld, loan);
                loan.Unlock();
                loan.Close();
                s.End();
                ld.Success = true;
                ld.Message = "Loan load successful.";           
            }
            catch (Exception ex)
            {
                ld.Success = false;
                ld.Message = ex.Message;
                sb.AppendLine("log something: Login Fail"+ ex.Message);
            }
            sb.AppendLine(System.DateTime.Now.ToString());
            sb.AppendLine("   ------------------------------------");
           
            File.AppendAllText("C://Temp//log.txt", sb.ToString());
            sb.Clear();
            return ld;
        }
     
        public Loan GetAttachmentbyId(Loan loan, LoanDetailResponse ld)
        {
            try
            {
                Directory.CreateDirectory("C:\\Temp\\" + loan.LoanName);

                foreach (Attachment att in loan.Attachments)
                {
                    att.SaveToDisk("C:\\Temp\\" + loan.LoanName+"\\" + att.Name);
                    ld.NumberOfAttachmentsSaved++;
                }
            }
            catch(Exception e)
            {
                sb.AppendLine("Attachment Exception" + e.Message);
            }
            return loan;
        }
        public LoanDetailResponse generateDataTable(string loanId, LoanDetailResponse ll, Loan loan)
        {
            ll.Street = loan.Fields["11"].Value!= null ?  loan.Fields["11"].Value.ToString(): "";
            ll.City = loan.Fields["12"].Value != null? loan.Fields["12"].Value.ToString() : "";
            ll.County = loan.Fields["13"].Value != null? loan.Fields["13"].Value.ToString() : "";
            ll.State = loan.Fields["14"].Value != null? loan.Fields["14"].Value.ToString() : "";
            ll.LoanToValue = loan.Fields["353"].Value != null? loan.Fields["353"].Value.ToString() : "";
            ll.LoanName = loan.Fields["37"].Value!= null ? loan.Fields["37"].Value.ToString(): "" + " " + loan.Fields["36"].Value != null ? loan.Fields["36"].Value.ToString() : "";
            ll.InterestRate = loan.Fields["3"].Value != null? loan.Fields["3"].Value.ToString() : "";
            ll.Term = loan.Fields["4"].Value != null? loan.Fields["4"].Value.ToString() : "";
            ll.MonthlyPayment = loan.Fields["5"].Value != null? loan.Fields["5"].Value.ToString() : "";
            ll.LoanId = loan.LoanNumber;

            return ll;
        }

        [HttpPost]
        [Route("api/CreateLoan")]
        public CreateLoanResponse CreateLoan(CreateLoanRequest request)
        {
            CreateLoanResponse response = new CreateLoanResponse();
            try
            {
                login();
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Unable to connect to server. Exited with Exception: " + e.Message;
                return response;
            }
            try
            {
                Loan loan = s.Loans.CreateNew();
                
                loan.LoanFolder = "Training";
                loan.LoanName = request.LoanName;
                loan.Fields["36"].Value = request.BorrowerFirstName;
                loan.Fields["37"].Value = request.BorrowerLastName;
                loan.Fields["11"].Value = request.Street;
                loan.Fields["12"].Value = request.City;
                loan.Fields["15"].Value = request.ZipCode;
                loan.Fields["13"].Value = request.County;
                loan.Fields["14"].Value = request.Street;
                loan.Fields["4"].Value = request.Term;
                loan.Fields["3"].Value = request.InterestRate;
                loan.Fields["5"].Value = request.MonthlyPayment;

                loan.Commit();

                // Trim the string to guid
                var n = 1;                
                string loanGuidInString= loan.Guid;

                if (loanGuidInString.Length > n * 2)
                    response.LoanGuid = Guid.Parse(loanGuidInString.Substring(n, loanGuidInString.Length - (n * 2)));
                else
                    response.LoanGuid = Guid.Empty;

                s.End();
                response.LoanFolder = loan.LoanFolder;
                response.LoanId = loan.LoanNumber;
                response.Success = true;
                response.Message = "Loan: " + request.LoanName + " saved successfully to folder: " + response.LoanFolder;
            }
            catch(Exception e)
            {
                response.Success = false;
                response.Message = "Unable to connect to server. Exited with Exception: " + e.Message;
                return response;
            }
            return response;
        }


        [HttpGet]
        [Route ("api/encompass/GetAttachmentsByLoanID")]
        public  GetAttachmentListResponse GetAttachementListByLoanID( string loanId)
        {
            GetAttachmentListResponse response = new GetAttachmentListResponse();
            try
            {
                login();
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Unable to connect to server. Exited with Exception: " + e.Message;
                return response;
            }
            try
            {                
                string id = "{" + loanId + "}";
                Loan loan = s.Loans.Open(id);
                sb.AppendLine("log something:Loan Name: " + loan.LoanName);
                response.Attachments = GetAttachmentListbyId(loan);
                response.AttachementCount = loan.Attachments.Count;
                response.LoanId = loanId;
                response.LoanName = loan.LoanName;
                response.LoanNumber = loan.LoanNumber;
                //response.Message = loan.Attachments[0].a
                response.Message = "Successfully retrieved attachement list on loan: " + loan.LoanName;
                response.Success = true;

                loan.Unlock();
                loan.Close();
                s.End();
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = "Unable to connect to server. Exited with Exception: " + e.Message;
                return response;
            }
            return response;
        }

        public List<AttachmentsList> GetAttachmentListbyId(Loan loan)
        {
            List<AttachmentsList> AttachmentList = new List<AttachmentsList>();
            try
            {              

                foreach (Attachment att in loan.Attachments)
                {
                    AttachmentsList a = new AttachmentsList();
                    a.Date = att.Date;
                    a.AttachmentName = att.Name;
                    AttachmentList.Add(a);
                }
            }
            catch (Exception e)
            {
                sb.AppendLine("Attachment Exception" + e.Message);
            }
            return AttachmentList;
        }

        [HttpGet]
        [Route("api/GetLoanSchema")]
        public LoanSchema GetLoanSchemaByLoanID(string loanId)
        {
            //            Loan ld = new Loan();
            LoanSchema l = new LoanSchema() ;


            string id = "{" + loanId + "}";
            try
            {                // Start the session
                login();

                sb.Append("log something: Login Success");
                // flush every 20 seconds as you do it               

                System.Web.Security.FormsAuthentication.SetAuthCookie(loginName, false);

                //Loan loan = s.Loans.Open("{fa8148a4-a482-4fa8-a6e6-ebbb3ae8e1cc}");
                l.loan = s.Loans.Open(id);
               l.Message =  new JavaScriptSerializer().Serialize(l.loan);

                sb.AppendLine("log something:Loan Name: " + l.loan.LoanName);
                l.Success = true;
              //  l.Message = "Success";
                return l;

                //GetAttachmentbyId(loan, ld);
                //ld = generateDataTable(id, ld, loan);
                //loan.Unlock();
                //loan.Close();
                //s.End();
                //ld.Success = true;
                //ld.Message = "Loan load successful.";
            }
            catch (Exception ex)
            {
                //ld.Success = false;
                //ld.Message = ex.Message;
                sb.AppendLine("log something: Login Fail" + ex.Message);
                l = null;
                l.Message = ex.Message;
                l.Success = false;
            }
            sb.AppendLine(System.DateTime.Now.ToString());
            sb.AppendLine("   ------------------------------------");

            File.AppendAllText("C://Temp//log.txt", sb.ToString());
            sb.Clear();
            return l;
        }
    }   
}
