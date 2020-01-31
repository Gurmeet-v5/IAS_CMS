using KISD.Areas.BlogAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class CommentContexts
    {
        private List<CommentModel> allComments;
        private XDocument CommentsData;

        public CommentContexts()
        {
            try
            {
                allComments = new List<CommentModel>();
                CommentsData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/comments.xml"));
                var Comments = from t in CommentsData.Descendants("Comment")
                            select new CommentModel(
                                (int)t.Element("CommentID"),
                                (int)t.Element("BlogID"),
                                t.Element("FullNameTxt").Value,
                            t.Element("EmailTxt").Value,
                            t.Element("PhoneNoTxt").Value,
                            t.Element("CommentDescriptionTxt").Value,
                            (DateTime)t.Element("PostedDate"),
                            (bool)t.Element("IsActiveInd"));

                allComments.AddRange(Comments.ToList<CommentModel>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets information from the data source for a Comment. 
        /// </summary>
        /// <returns>A Comment object populated with all Comment's information from the data source.</returns>
        public IEnumerable<CommentModel> GetComments()
        {
            return allComments;
        }
        /// <summary>
        /// Gets information from the data source for a Comment. 
        /// </summary>
        /// <param name="CommentName">The name of the Comment to get information for.</param>
        /// <returns>A Comment object populated with the specified Comment's information from the data source.</returns>
        public CommentModel GetComment(int CommentID)
        {
            return allComments.Find(item => item.CommentID == CommentID);
        }

        public void AddComment(CommentModel _CommentsModel)
        {
            _CommentsModel.CommentID = (int)(from S in CommentsData.Descendants("Comment") orderby (short)S.Element("CommentID") descending select (short)S.Element("CommentID")).FirstOrDefault() + 1;
            CommentsData.Root.Add(new XElement("Comment", new XElement("CommentID", _CommentsModel.CommentID),
                               new XElement("BlogID", _CommentsModel.BlogID),
                               new XElement("FullNameTxt", _CommentsModel.FullNameTxt),
                               new XElement("EmailTxt", _CommentsModel.EmailTxt),
                               new XElement("PhoneNoTxt", _CommentsModel.PhoneNoTxt),
                               new XElement("CommentDescriptionTxt", _CommentsModel.CommentDescriptionTxt),
                               new XElement("PostedDate", _CommentsModel.PostedDate),
                               new XElement("IsActiveInd", _CommentsModel.IsActiveInd)));

            CommentsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/comments.xml"));
        }

        public void EditComment(CommentModel _CommentModel)
        {
            try
            {
                XElement node = CommentsData.Root.Elements("Comment").Where(i => (int)i.Element("CommentID") == _CommentModel.CommentID).FirstOrDefault();

                node.SetElementValue("IsActiveInd", _CommentModel.IsActiveInd);
                CommentsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/comments.xml"));
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }

        public void DeleteComments(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    CommentsData.Root.Elements("Comment").Where(i => (int)i.Element("CommentID") == id).Remove();

                    CommentsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/comments.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }

        public void DeleteBlogComments(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    CommentsData.Root.Elements("Comment").Where(i => (int)i.Element("BlogID") == id).Remove();

                    CommentsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/comments.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }
    }
}