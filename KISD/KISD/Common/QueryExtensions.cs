using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using System.Web.Mvc;

namespace KISD.Common
{
    public static class QueryExtension
    {
        /// <summary>
        /// Used to fill data in grid or paged model.
        /// Used to sort data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="dataValueField"></param>
        /// <param name="dataTextField"></param>
        /// <param name="selectedValue"></param>
        /// <returns></returns>
        public static SelectList ToSelectList<T>(this IQueryable<T> query, string dataValueField, string dataTextField, object selectedValue)
        {
            return new SelectList(query, dataValueField, dataTextField, selectedValue ?? -1);
        }

    }

    public class MailUtilityBAL
    {
        public string SendEmail(string fromEmail, string emailAddress, string emailBody, string emailSubject)
        {
            //var fromEmail = ConfigurationManager.AppSettings["FromEmail"].ToString();
            var mailServer = ConfigurationManager.AppSettings["Host"].ToString();
            var Authreq = Convert.ToInt16(ConfigurationManager.AppSettings["Authorization"].ToString());
            var userName = ConfigurationManager.AppSettings["UserName"].ToString();
            var password = ConfigurationManager.AppSettings["Password"].ToString();

            var myMailUtilityDAL = new MailUtilityDAL();
            myMailUtilityDAL.Authreq = Authreq;
            myMailUtilityDAL.Body = emailBody;
            myMailUtilityDAL.FromEmail = fromEmail;
            myMailUtilityDAL.Subject = emailSubject;
            myMailUtilityDAL.ToEmail = emailAddress;
            myMailUtilityDAL.Username = userName;
            myMailUtilityDAL.Password = password;
            myMailUtilityDAL.SmtpServer = mailServer;
            return myMailUtilityDAL.SendEmailAsync();
        }
    }

    public class MailUtilityDAL
    {
        private string subject;
        private string smtpServer;
        private string fromEmail;
        private string body;
        private string toEmail;
        private int authreq;
        private string username;
        private string password;

        public string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                subject = value;
            }
        }

        public string SmtpServer
        {
            get
            {
                return smtpServer;
            }
            set
            {
                smtpServer = value;
            }
        }

        public string FromEmail
        {
            get
            {
                return fromEmail;
            }

            set
            {
                fromEmail = value;
            }
        }

        public string ToEmail
        {
            get
            {
                return toEmail;
            }

            set
            {
                toEmail = value;
            }
        }

        public string Body
        {
            get
            {
                return body;
            }

            set
            {
                body = value;
            }
        }

        public int Authreq
        {
            get
            {
                return authreq;
            }

            set
            {
                authreq = value;
            }
        }

        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                username = value;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value;
            }
        }

        public string SendEmail()
        {
            try
            {
                var mMailMessage = new MailMessage(this.fromEmail, this.toEmail);
                mMailMessage.From = new System.Net.Mail.MailAddress(this.fromEmail, "KISD");
                mMailMessage.Subject = this.Subject;
                mMailMessage.Body = this.Body;
                mMailMessage.IsBodyHtml = true;
                mMailMessage.Priority = MailPriority.Normal;
                var mSmtpClient = new SmtpClient();
                mSmtpClient.Host = this.smtpServer;
                if (this.authreq == 1)
                {
                    mSmtpClient.Credentials = new NetworkCredential(this.username, this.password);
                }
                mSmtpClient.Send(mMailMessage);
            }
            catch
            {
                return "Send error";
            }
            return "sent";
        }

        public delegate string SendEmailDelegate();

        public void GetResultsOnCallback(IAsyncResult ar)
        {
            SendEmailDelegate del = (SendEmailDelegate)
             ((AsyncResult)ar).AsyncDelegate;
            try
            {
                string result;
                result = del.EndInvoke(ar);
                Debug.WriteLine("\nOn CallBack: result is " + result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("\nOn CallBack, problem occurred: " + ex.Message);
            }
        }

        public string SendEmailAsync()
        {
            try
            {
                SendEmailDelegate dc = new SendEmailDelegate(this.SendEmail);
                AsyncCallback cb = new AsyncCallback(this.GetResultsOnCallback);
                IAsyncResult ar = dc.BeginInvoke(cb, null);
                return "ok";
            }
            catch
            {
                return "Not ok";
            }
        }
    }
}