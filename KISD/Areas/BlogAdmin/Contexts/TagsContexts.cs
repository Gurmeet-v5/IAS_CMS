using KISD.Areas.BlogAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class TagsContexts
    {
        private List<TagsModel> allTags;
        private XDocument TagsData;
        public TagsContexts()
        {
            try
            {
                allTags = new List<TagsModel>();
                TagsData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/tags.xml"));
                var Tags = from t in TagsData.Descendants("Tag")
                                 select new TagsModel(
                                     (int)t.Element("TagID"),
                                     t.Element("TagNameTxt").Value);

                allTags.AddRange(Tags.ToList<TagsModel>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets information from the data source for a Tag. 
        /// </summary>
        /// <returns>A Tag object populated with all Tag's information from the data source.</returns>
        public IEnumerable<TagsModel> GetTags()
        {
            return allTags;
        }
        /// <summary>
        /// Gets information from the data source for a Tag. 
        /// </summary>
        /// <param name="TagName">The name of the Tag to get information for.</param>
        /// <returns>A Tag object populated with the specified Tag's information from the data source.</returns>
        public TagsModel GetTag(string TagName)
        {
            return allTags.Find(item => item.TagNameTxt == TagName);
        }

     
        public void AddTags(TagsModel _TagsModel)
        {
            _TagsModel.TagID = (short)((int)(from S in TagsData.Descendants("Tag") orderby (short)S.Element("TagID") descending select (short)S.Element("TagID")).FirstOrDefault() + 1);
            TagsData.Root.Add(new XElement("Tag", new XElement("TagID", _TagsModel.TagID),
                               new XElement("TagNameTxt", _TagsModel.TagNameTxt)));

            TagsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/Tags.xml"));
        }

        public void EditTags(TagsModel _TagsModel)
        {
            try
            {
                XElement node = TagsData.Root.Elements("Tag").Where(i => (int)i.Element("TagID") == _TagsModel.TagID).FirstOrDefault();

                node.SetElementValue("TagNameTxt", _TagsModel.TagNameTxt);
                TagsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/Tags.xml"));
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }

        public void DeleteTags(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    TagsData.Root.Elements("Tag").Where(i => (int)i.Element("TagID") == id).Remove();

                    TagsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/Tags.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }
       
    }
}