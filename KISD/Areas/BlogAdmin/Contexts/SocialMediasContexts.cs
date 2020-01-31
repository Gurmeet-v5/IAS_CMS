using KISD.Areas.BlogAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class SocialMediasContexts
    {
        private List<SocialMediasModel> allSocialMedias;
        private XDocument SocialMediasData;
        public SocialMediasContexts()
        {
            try
            {
                allSocialMedias = new List<SocialMediasModel>();
                SocialMediasData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/socialmedia.xml"));
                var SocialMedias = from t in SocialMediasData.Descendants("SocialMedia")
                                 select new SocialMediasModel(
                                     (int)t.Element("SocialMediaID"),
                                     t.Element("SocialMediaNameTxt").Value);

                allSocialMedias.AddRange(SocialMedias.ToList<SocialMediasModel>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets information from the data source for a Social Media. 
        /// </summary>
        /// <returns>A Social Media object populated with all Social Media's information from the data source.</returns>
        public IEnumerable<SocialMediasModel> GetSocialMedias()
        {
            return allSocialMedias;
        }
        /// <summary>
        /// Gets information from the data source for a SocialMedia. 
        /// </summary>
        /// <param name="SocialMediaName">The name of the SocialMedia to get information for.</param>
        /// <returns>A SocialMedia object populated with the specified SocialMedia's information from the data source.</returns>
        public SocialMediasModel GetSocialMedia(string SocialMediaName)
        {
            return allSocialMedias.Find(item => item.SocialMediaNameTxt == SocialMediaName);
        }

        public void AddSocialMedias(SocialMediasModel _SocialMediasModel)
        {
            _SocialMediasModel.SocialMediaID = (short)((int)(from S in SocialMediasData.Descendants("SocialMedia") orderby (short)S.Element("SocialMediaID") descending select (short)S.Element("SocialMediaID")).FirstOrDefault() + 1);
            SocialMediasData.Root.Add(new XElement("SocialMedia", new XElement("SocialMediaID", _SocialMediasModel.SocialMediaID),
                               new XElement("SocialMediaNameTxt", _SocialMediasModel.SocialMediaNameTxt)));

            SocialMediasData.Save(HttpContext.Current.Server.MapPath("~/App_Data/SocialMedia.xml"));
        }

        public void EditSocialMedias(SocialMediasModel _SocialMediasModel)
        {
            try
            {
                XElement node = SocialMediasData.Root.Elements("SocialMedia").Where(i => (int)i.Element("SocialMediaID") == _SocialMediasModel.SocialMediaID).FirstOrDefault();

                node.SetElementValue("SocialMediaNameTxt", _SocialMediasModel.SocialMediaNameTxt);
                SocialMediasData.Save(HttpContext.Current.Server.MapPath("~/App_Data/SocialMedia.xml"));
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }

        public void DeleteSocialMedias(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    SocialMediasData.Root.Elements("SocialMedia").Where(i => (int)i.Element("SocialMediaID") == id).Remove();

                    SocialMediasData.Save(HttpContext.Current.Server.MapPath("~/App_Data/socialmedia.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }
    }
}