using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using KISD.Areas.BlogAdmin.Models;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class EmailsContexts
    {
        private List<EmailsModel> allEmails;
        private XDocument EmailsData;
        public EmailsContexts()
        {
            try
            {
                allEmails = new List<EmailsModel>();
                EmailsData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/emails.xml"));
                var Emails = from t in EmailsData.Descendants("Email")
                            select new EmailsModel(
                                (short)t.Element("EmailID"),
                                t.Element("EmailTxt").Value,
                            (int)t.Element("EmailTypeID"));

                allEmails.AddRange(Emails.ToList<EmailsModel>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets information from the data source for a Email. 
        /// </summary>
        /// <returns>A Email object populated with all Email's information from the data source.</returns>
        public IEnumerable<EmailsModel> GetEmails()
        {
            return allEmails;
        }

        /// <summary>
        /// Gets information from the data source for a Email. 
        /// </summary>
        /// <param name="EmailTypeID">The name of the Email to get information for.</param>
        /// <returns>A Email object populated with the specified Email's information from the data source.</returns>
        public IEnumerable<EmailsModel> GetEmail(int? EmailTypeID)
        {
            return allEmails.Where(item => item.EmailTypeID == EmailTypeID);
        }
        /// <summary>
        /// Gets information from the data source for a Email. 
        /// </summary>
        /// <param name="EmailName">The name of the Email to get information for.</param>
        /// <returns>A Email object populated with the specified Email's information from the data source.</returns>
        public EmailsModel GetEmail(string Email)
        {
            return allEmails.Find(item => item.EmailTxt == Email);
        }




        public void AddEmail(EmailsModel _EmailsModel)
        {
            _EmailsModel.EmailID = (short)((int)(from S in EmailsData.Descendants("Email") orderby (short)S.Element("EmailID") descending select (short)S.Element("EmailID")).FirstOrDefault() + 1);
            EmailsData.Root.Add(new XElement("Email", new XElement("EmailID", _EmailsModel.EmailID),
                               new XElement("EmailTxt", _EmailsModel.EmailTxt),
                               new XElement("EmailTypeID", _EmailsModel.EmailTypeID)));

            EmailsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/emails.xml"));
        }

        public void EditEmails(EmailsModel _EmailsModel)
        {
            try
            {
                XElement node = EmailsData.Root.Elements("Email").Where(i => (int)i.Element("EmailID") == _EmailsModel.EmailID).FirstOrDefault();

                node.SetElementValue("EmailTxt", _EmailsModel.EmailTxt);
                node.SetElementValue("EmailTypeID", _EmailsModel.EmailTypeID);
                EmailsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/emails.xml"));
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }

        public void DeleteEmails(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    EmailsData.Root.Elements("Email").Where(i => (int)i.Element("EmailID") == id).Remove();

                    EmailsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/emails.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }

        public string GetEmailType(int EmailType)
        {
            List<EmailTypeModel> allEmailsType=new List<EmailTypeModel>();
            XDocument EmailTypeData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/emailtypes.xml"));
            var EmailsType = from t in EmailTypeData.Descendants("EmailType")
                         select new EmailTypeModel(
                             (int)t.Element("EmailTypeID"),
                             t.Element("EmailTypeNameTxt").Value);

            allEmailsType.AddRange(EmailsType.ToList<EmailTypeModel>());
            return allEmailsType.Where(x => x.EmailTypeID == EmailType).Select(x => x.EmailTypeNameTxt).FirstOrDefault();
        }

        public enum EmailType : int
        {
            From_Email = 1,
            To_Email = 2
        }
    }
}