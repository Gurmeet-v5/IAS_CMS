using System;
using System.Linq;

namespace KISD.Areas.Admin.Models
{
    public class EmailModel
    {
        public long EmailID { get; set; }
        public string EmailTxt { get; set; }
        public long EmailTypeID { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
    }
    public class FormEmailModel
    {
        public int FormEmailID { get; set; }
        public int EmailID { get; set; }
        public int ContentTypeID { get; set; }
    }

    public class EmailService
    {
        private db_KISDEntities _context;
        public EmailService()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// Get Queryable model of emails for defined type
        /// </summary>
        /// <param name="EmailType"></param>
        /// <returns></returns>
        public IQueryable<EmailModel> GetEmails(int EmailType)
        {
            var query = from a in GetAllEmails(EmailType)
                        select new EmailModel
                        {
                            EmailID = a.EmailID,
                            EmailTxt = a.EmailTxt,
                            EmailTypeID = a.EmailTypeID,
                            CreateDate=a.CreateDate,
                            CreateByID=a.CreateByID,
                            LastModifyDate=a.LastModifyDate,
                            LastModifyByID=a.LastModifyByID,
                            IsDeletedInd=a.IsDeletedInd
                        };
            return query;
        }
        /// <summary>
        /// Get all Emails of defined type
        /// </summary>
        /// <param name="EmailType"></param>
        /// <returns></returns>
        public IQueryable<Email> GetAllEmails(int EmailType)
        {
            return _context.Emails.Where(x => x.EmailTypeID == EmailType && x.IsDeletedInd==false);
        }

        /// <summary>
        /// Get all Emails of defined type
        /// </summary>
        /// <param name="EmailType"></param>
        /// <returns></returns>
        public string GetEmailType(long EmailType)
        {
            return _context.EmailTypes.Where(x => x.EmailTypeID == EmailType).Select(x => x.EmailTypeNameTxt).FirstOrDefault();
        }

        /// <summary>
        ///  Emails of defined type
        /// </summary>
        /// <param name="EmailType"></param>
        /// <returns></returns>
        public enum EmailType : int
        {
            From_Email = 1,
            To_Email = 2
        }
    }
}
