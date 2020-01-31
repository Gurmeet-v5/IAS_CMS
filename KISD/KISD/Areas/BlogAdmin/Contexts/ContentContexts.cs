using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KISD.Areas.BlogAdmin.Models;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class ContentContexts
    {
        private List<ContentModel> allContents;
        private XDocument ContentsData;
        public ContentContexts()
        {
            try
            {
                allContents = new List<ContentModel>();
                ContentsData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/contents.xml"));
                var Contents = from t in ContentsData.Descendants("Content")
                               select new ContentModel(
                                   (short)t.Element("ContentID"),
                                   t.Element("ContentTxt").Value,
                               (int)t.Element("ContentTypeID"),
                               t.Element("TitleTxt").Value,
                               t.Element("AltImgTxt").Value,
                               t.Element("ImagePathTxt").Value,
                                t.Element("MetaTitleTxt").Value,
                               t.Element("MetaDescriptionTxt").Value
                               );

                allContents.AddRange(Contents.ToList<ContentModel>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets information from the data source for a Content. 
        /// </summary>
        /// <returns>A Content object populated with all Content's information from the data source.</returns>
        public IEnumerable<ContentModel> GetContents()
        {
            return allContents;
        }

        /// <summary>
        /// Gets information from the data source for a Content. 
        /// </summary>
        /// <param name="ContentTypeID">The name of the Content to get information for.</param>
        /// <returns>A Content object populated with the specified Content's information from the data source.</returns>
        public IEnumerable<ContentModel> GetContent(int? ContentTypeID)
        {
            return allContents.Where(item => item.ContentTypeID == ContentTypeID);
        }
        /// <summary>
        /// Gets information from the data source for a Content. 
        /// </summary>
        /// <param name="ContentName">The name of the Content to get information for.</param>
        /// <returns>A Content object populated with the specified Content's information from the data source.</returns>
        public void AddContent(ContentModel _ContentsModel)
        {
            _ContentsModel.ContentID = (short)((int)(from S in ContentsData.Descendants("Content") orderby (short)S.Element("ContentID") descending select (short)S.Element("ContentID")).FirstOrDefault() + 1);
            ContentsData.Root.Add(new XElement("Content", new XElement("ContentID", _ContentsModel.ContentID),
                               new XElement("ContentTxt", !string.IsNullOrEmpty(_ContentsModel.DescriptionTxt) ? _ContentsModel.DescriptionTxt : "" ),
                               new XElement("ContentTypeID", _ContentsModel.ContentTypeID),
                               new XElement("TitleTxt", !string.IsNullOrEmpty(_ContentsModel.TitleTxt) ? _ContentsModel.TitleTxt : ""),
                              new XElement("AltImgTxt", !string.IsNullOrEmpty(_ContentsModel.AltImgTxt) ? _ContentsModel.AltImgTxt : ""),
                              new XElement("ImagePathTxt", !string.IsNullOrEmpty(_ContentsModel.ImagePathTxt) ? _ContentsModel.ImagePathTxt : "")
                               ));
            if (_ContentsModel.ContentTypeID == Convert.ToInt32(ContentContexts.ContentType.Header))
            {
                ContentsData.Add(
                              new XElement("MetaTitleTxt", !string.IsNullOrEmpty(_ContentsModel.MetaTitleTxt) ? _ContentsModel.MetaTitleTxt : ""),
                              new XElement("MetaDescriptionTxt", !string.IsNullOrEmpty(_ContentsModel.MetaDescriptionTxt) ? _ContentsModel.MetaDescriptionTxt : "")
                              );
            }
            ContentsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/contents.xml"));
        }

        public void EditContents(ContentModel _ContentsModel)
        {
            try
            {
                XElement node = ContentsData.Root.Elements("Content").Where(i => (int)i.Element("ContentID") == _ContentsModel.ContentID).FirstOrDefault();
                node.SetElementValue("ContentTxt", !string.IsNullOrEmpty(_ContentsModel.DescriptionTxt) ? _ContentsModel.DescriptionTxt : "" );
                node.SetElementValue("ContentTypeID", _ContentsModel.ContentTypeID);
                node.SetElementValue("TitleTxt", !string.IsNullOrEmpty(_ContentsModel.TitleTxt) ? _ContentsModel.TitleTxt : "");
                node.SetElementValue("AltImgTxt", !string.IsNullOrEmpty(_ContentsModel.AltImgTxt) ? _ContentsModel.AltImgTxt : "");
                node.SetElementValue("ImagePathTxt", !string.IsNullOrEmpty(_ContentsModel.ImagePathTxt) ? _ContentsModel.ImagePathTxt : ""); 
                if (_ContentsModel.ContentTypeID == Convert.ToInt32(ContentContexts.ContentType.Header))
                {
                    node.SetElementValue("MetaTitleTxt", !string.IsNullOrEmpty(_ContentsModel.MetaTitleTxt) ? _ContentsModel.MetaTitleTxt : "");
                    node.SetElementValue("MetaDescriptionTxt", !string.IsNullOrEmpty(_ContentsModel.MetaDescriptionTxt) ? _ContentsModel.MetaDescriptionTxt : "");
                }
                ContentsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/contents.xml"));
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }
        }

        public void DeleteContents(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    ContentsData.Root.Elements("Content").Where(i => (int)i.Element("ContentID") == id).Remove();
                    ContentsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/contents.xml"));
                }
                catch (Exception)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public string GetContentType(int ContentType)
        {
            List<ContentTypeModel> allContentsType = new List<ContentTypeModel>();
            XDocument ContentTypeData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/Contenttypes.xml"));
            var ContentsType = from t in ContentTypeData.Descendants("ContentType")
                               select new ContentTypeModel(
                                   (int)t.Element("ContentTypeID"),
                                   t.Element("ContentTypeNameTxt").Value);

            allContentsType.AddRange(ContentsType.ToList<ContentTypeModel>());
            return allContentsType.Where(x => x.ContentTypeID == ContentType).Select(x => x.TitleTxt).FirstOrDefault();
        }

        public enum ContentType : int
        {
            Header = 1,
            Footer = 2,
            BasicSetting = 3,
            EmailSetting = 4,
            ThemeSetting = 5,
        }
    }
}