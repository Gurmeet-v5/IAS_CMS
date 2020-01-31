using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KISD.Areas.BlogAdmin.Models;
using System.Xml.Linq;
using System.Web.Mvc;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class CategoriesContexts
    {
        private List<CategoriesModel> allCategories;
        private XDocument CategoriesData;
        public CategoriesContexts()
        {
            try
            {
                allCategories = new List<CategoriesModel>();
                CategoriesData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/categories.xml"));
                var Categories = from t in CategoriesData.Descendants("Category")
                            select new CategoriesModel(
                                (int)t.Element("CategoryID"),
                                t.Element("CategoryNameTxt").Value);

                allCategories.AddRange(Categories.ToList<CategoriesModel>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets information from the data source for a Categorie. 
        /// </summary>
        /// <returns>A Categorie object populated with all Categorie's information from the data source.</returns>
        public IEnumerable<CategoriesModel> GetCategories()
        {
            return allCategories;
        }
        /// <summary>
        /// Gets the list of selected categories 
        /// </summary>
        /// <param name="value">Category ID</param>
        /// <returns></returns>
        public IEnumerable<CategoriesModel> GetSelectedCategories(string value)
        {
            var objCategoriesModel = new CategoriesModel();
            objCategoriesModel.CategoryNameTxt = "-- Select Category --";
            objCategoriesModel.CategoryID = 0;
            allCategories.Insert(0, objCategoriesModel);
            List<SelectListItem> Selectitems = new List<SelectListItem>();
            foreach (var item in allCategories)
            {
                SelectListItem data = new SelectListItem();
                data.Text = item.CategoryNameTxt;
                data.Value = Convert.ToString(item.CategoryID);
                Selectitems.Add(data);
            }
            if (!string.IsNullOrEmpty(value))
            {
                Selectitems.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }

            return allCategories;
        }
        /// <summary>
        /// Gets information from the data source for a Categorie. 
        /// </summary>
        /// <param name="CategorieName">The name of the Categorie to get information for.</param>
        /// <returns>A Categorie object populated with the specified Categorie's information from the data source.</returns>
        public CategoriesModel GetCategory(string CategoryName)
        {
            return allCategories.Find(item => item.CategoryNameTxt == CategoryName);
        }




        public void AddCategories(CategoriesModel _CategoriesModel)
        {
            _CategoriesModel.CategoryID = (short)((int)(from S in CategoriesData.Descendants("Category") orderby (short)S.Element("CategoryID") descending select (short)S.Element("CategoryID")).FirstOrDefault() + 1);
            CategoriesData.Root.Add(new XElement("Category", new XElement("CategoryID", _CategoriesModel.CategoryID),
                               new XElement("CategoryNameTxt", _CategoriesModel.CategoryNameTxt)));

            CategoriesData.Save(HttpContext.Current.Server.MapPath("~/App_Data/categories.xml"));
        }

        public void EditCategories(CategoriesModel _CategoriesModel)
        {
            try
            {
                XElement node = CategoriesData.Root.Elements("Category").Where(i => (int)i.Element("CategoryID") == _CategoriesModel.CategoryID).FirstOrDefault();

                node.SetElementValue("CategoryNameTxt", _CategoriesModel.CategoryNameTxt);
                CategoriesData.Save(HttpContext.Current.Server.MapPath("~/App_Data/categories.xml"));
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }

        public void DeleteCategories(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    CategoriesData.Root.Elements("Category").Where(i => (int)i.Element("CategoryID") == id).Remove();

                    CategoriesData.Save(HttpContext.Current.Server.MapPath("~/App_Data/categories.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }
    }
}