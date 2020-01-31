using KISD.Areas.BlogAdmin.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class SettingContexts
    {
        private List<SettingModel> allSettings;
        private XDocument SettingsData;
        public SettingContexts()
        {
            try
            {
                allSettings = new List<SettingModel>();
                SettingsData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/Settings.xml"));
                var Settings = from t in SettingsData.Descendants("Setting")
                            select new SettingModel(
                                (int)t.Element("SettingID"),
                                (int)t.Element("PostPerPage"),
                                (bool)t.Element("IsSearchEnabled"),
                            t.Element("ThemesPath").Value,
                            t.Element("SendingEmail").Value,
                            t.Element("ReceivingEmail").Value,
                            (int)t.Element("CommentPerPost"),
                            !string.IsNullOrEmpty(t.Element("PagingColor").Value)? t.Element("PagingColor").Value:"",
                            !string.IsNullOrEmpty(t.Element("PagingActiveColor").Value)? t.Element("PagingActiveColor").Value:"",
                            !string.IsNullOrEmpty(t.Element("PagingHoverColor").Value)? t.Element("PagingHoverColor").Value:"",
                            !string.IsNullOrEmpty(t.Element("ButtonColor").Value) ? t.Element("ButtonColor").Value : "",
                            !string.IsNullOrEmpty(t.Element("NevigationBarColor").Value) ? t.Element("NevigationBarColor").Value : "",
                            !string.IsNullOrEmpty(t.Element("NevigationBarFontFamily").Value) ? t.Element("NevigationBarFontFamily").Value : "",
                            !string.IsNullOrEmpty(t.Element("NevigationBarFontFamilyTxt").Value) ? t.Element("NevigationBarFontFamilyTxt").Value : "",
                            !string.IsNullOrEmpty(t.Element("NevigationBarHoverColor").Value) ? t.Element("NevigationBarHoverColor").Value : "",
                            !string.IsNullOrEmpty(t.Element("NevigationBarFontColor").Value) ? t.Element("NevigationBarFontColor").Value : "",
                            !string.IsNullOrEmpty(t.Element("NevigationBarTextFontSize").Value) ? t.Element("NevigationBarTextFontSize").Value : "",
                            !string.IsNullOrEmpty(t.Element("NevigationBarTextFontStyle").Value) ? t.Element("NevigationBarTextFontStyle").Value : "",
                            !string.IsNullOrEmpty(t.Element("FooterColor").Value) ? t.Element("FooterColor").Value : "",
                            !string.IsNullOrEmpty(t.Element("SidebarTitleBackgroundColor").Value) ? t.Element("SidebarTitleBackgroundColor").Value : "",
                            !string.IsNullOrEmpty(t.Element("SidebarTitleFontFamily").Value) ? t.Element("SidebarTitleFontFamily").Value : "",
                            !string.IsNullOrEmpty(t.Element("SidebarTitleFontFamilyTxt").Value) ? t.Element("SidebarTitleFontFamilyTxt").Value : "",
                            !string.IsNullOrEmpty(t.Element("SidebarTitleFontcolor").Value) ? t.Element("SidebarTitleFontcolor").Value : "",
                            !string.IsNullOrEmpty(t.Element("SidebarTitleFontSize").Value) ? t.Element("SidebarTitleFontSize").Value : "",
                            //!string.IsNullOrEmpty(t.Element("SideBarCategoryTextFontStyle").Value) ? t.Element("SideBarCategoryTextFontStyle").Value : "",
                            !string.IsNullOrEmpty(t.Element("PostTitleFontColor").Value) ? t.Element("PostTitleFontColor").Value : "",
                            !string.IsNullOrEmpty(t.Element("PostTitleFontFamily").Value) ? t.Element("PostTitleFontFamily").Value : "",
                            !string.IsNullOrEmpty(t.Element("PostTitleFontFamilyTxt").Value) ? t.Element("PostTitleFontFamilyTxt").Value : "",
                            //!string.IsNullOrEmpty(t.Element("SidebarTitleFontcolor").Value) ? t.Element("SidebarTitleFontcolor").Value : "",
                            !string.IsNullOrEmpty(t.Element("PostTitleFontSize").Value) ? t.Element("PostTitleFontSize").Value : "",
                             //!string.IsNullOrEmpty(t.Element("SideBarTagTextFontStyle").Value) ? t.Element("SideBarTagTextFontStyle").Value : ""
                             (bool)t.Element("IsCommentEnabled"),
                             (bool)t.Element("IsSocialSharingEnabled")
                            );
                allSettings.AddRange(Settings.ToList<SettingModel>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
            //catch (DbEntityValidationException ex)// Get Entity validation errors with property name and error message
            //{
            //    foreach (var entityValidationErrors in ex.EntityValidationErrors)
            //    {
            //        foreach (var validationError in entityValidationErrors.ValidationErrors)
            //        {
            //          var message = ("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
            //        }
            //    }
            //}
        }
        /// <summary>
        /// Gets information from the data source for a Setting. 
        /// </summary>
        /// <returns>A Setting object populated with all Setting's information from the data source.</returns>
        public IEnumerable<SettingModel> GetSettings()
        {
            return allSettings;
        }
        
        public void AddSetting(SettingModel _SettingsModel)
        {
            _SettingsModel.SettingID = (int)(from S in SettingsData.Descendants("Setting") orderby (short)S.Element("SettingID") descending select (short)S.Element("SettingID")).FirstOrDefault() + 1;
            SettingsData.Root.Add(new XElement("Setting",  new XElement("SettingID", _SettingsModel.SettingID),
                                new XElement("PostPerPage", !string.IsNullOrEmpty(Convert.ToString(_SettingsModel.PostPerPage)) ? _SettingsModel.PostPerPage : 10 ),
                               new XElement("IsSearchEnabled", _SettingsModel.IsSearchEnabled),
                               new XElement("ThemesPath", !string.IsNullOrEmpty(_SettingsModel.ThemesPath) ? _SettingsModel.ThemesPath : ""),
                               new XElement("SendingEmail", !string.IsNullOrEmpty(_SettingsModel.SendingEmail) ? _SettingsModel.SendingEmail : ""),
                               new XElement("ReceivingEmail", !string.IsNullOrEmpty(_SettingsModel.ReceivingEmail) ? _SettingsModel.ReceivingEmail : ""),
                               new XElement("CommentPerPost", !string.IsNullOrEmpty(Convert.ToString(_SettingsModel.CommentPerPost)) ? _SettingsModel.CommentPerPost : 5),
                               new XElement("PagingColor", !string.IsNullOrEmpty(_SettingsModel.PagingColor) ? _SettingsModel.PagingColor : ""),
                               new XElement("PagingActiveColor", !string.IsNullOrEmpty(_SettingsModel.PagingActiveColor) ? _SettingsModel.PagingActiveColor : ""),
                               new XElement("PagingHoverColor", !string.IsNullOrEmpty(_SettingsModel.PagingHoverColor) ? _SettingsModel.PagingHoverColor : ""),
                               new XElement("ButtonColor", !string.IsNullOrEmpty(_SettingsModel.PagingHoverColor) ? _SettingsModel.PagingHoverColor : ""),
                               new XElement("NevigationBarColor", !string.IsNullOrEmpty(_SettingsModel.NevigationBarColor) ? _SettingsModel.NevigationBarColor : ""),
                               new XElement("NevigationBarFontFamily", !string.IsNullOrEmpty(_SettingsModel.NevigationBarFontFamily) ? _SettingsModel.NevigationBarFontFamily : ""),
                                new XElement("NevigationBarFontFamilyTxt", !string.IsNullOrEmpty(_SettingsModel.NevigationBarFontFamilyTxt) ? _SettingsModel.NevigationBarFontFamilyTxt : ""),
                               new XElement("NevigationBarHoverColor", !string.IsNullOrEmpty(_SettingsModel.NevigationBarHoverColor) ? _SettingsModel.NevigationBarHoverColor : ""),
                               new XElement("NevigationBarFontColor", !string.IsNullOrEmpty(_SettingsModel.NevigationBarFontColor) ? _SettingsModel.NevigationBarFontColor : ""),
                               new XElement("NevigationBarTextFontSize", !string.IsNullOrEmpty(_SettingsModel.NevigationBarTextFontSize) ? _SettingsModel.NevigationBarTextFontSize : ""),
                               new XElement("NevigationBarTextFontStyle", !string.IsNullOrEmpty(_SettingsModel.NevigationBarTextFontStyle) ? _SettingsModel.NevigationBarTextFontStyle : ""),
                               new XElement("FooterColor", !string.IsNullOrEmpty(_SettingsModel.FooterColor) ? _SettingsModel.FooterColor : ""),
                               new XElement("SidebarTitleBackgroundColor", !string.IsNullOrEmpty(_SettingsModel.SidebarTitleBackgroundColor) ? _SettingsModel.SidebarTitleBackgroundColor : ""),
                               new XElement("SidebarTitleFontFamily", !string.IsNullOrEmpty(_SettingsModel.SidebarTitleFontFamily) ? _SettingsModel.SidebarTitleFontFamily : ""),
                                 new XElement("SidebarTitleFontFamilyTxt", !string.IsNullOrEmpty(_SettingsModel.SidebarTitleFontFamilyTxt) ? _SettingsModel.SidebarTitleFontFamilyTxt : ""),
                               new XElement("SidebarTitleFontcolor", !string.IsNullOrEmpty(_SettingsModel.SidebarTitleFontcolor) ? _SettingsModel.SidebarTitleFontcolor : ""),
                               new XElement("SidebarTitleFontSize", !string.IsNullOrEmpty(_SettingsModel.SidebarTitleFontSize) ? _SettingsModel.SidebarTitleFontSize : ""),
                               //new XElement("SideBarCategoryTextFontStyle", _SettingsModel.SideBarCategoryTextFontStyle),
                               new XElement("PostTitleFontColor", !string.IsNullOrEmpty(_SettingsModel.PostTitleFontColor) ? _SettingsModel.PostTitleFontColor : ""),
                               new XElement("PostTitleFontFamily", !string.IsNullOrEmpty(_SettingsModel.PostTitleFontFamily) ? _SettingsModel.PostTitleFontFamily : ""),
                               new XElement("PostTitleFontFamilyTxt", !string.IsNullOrEmpty(_SettingsModel.PostTitleFontFamilyTxt) ? _SettingsModel.PostTitleFontFamilyTxt : ""),
                               //new XElement("SidebarTitleFontcolor", _SettingsModel.SidebarTitleFontcolor),
                               new XElement("PostTitleFontSize", !string.IsNullOrEmpty(_SettingsModel.PostTitleFontSize) ? _SettingsModel.PostTitleFontSize : ""),
                               //new XElement("SideBarTagTextFontStyle", _SettingsModel.SideBarTagTextFontStyle)
                               new XElement("IsCommentEnabled",  _SettingsModel.IsCommentEnabled),
                               new XElement("IsSocialSharingEnabled",  _SettingsModel.IsSocialSharingEnabled)
                               ));

            SettingsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/Settings.xml"));
        }

        public void EditSetting(SettingModel _SettingModel)
        {
            try
            {
                XElement node = SettingsData.Root.Elements("Setting").Where(i => (int)i.Element("SettingID") == _SettingModel.SettingID).FirstOrDefault();
                if (_SettingModel.ContentType == Convert.ToInt32(ContentContexts.ContentType.BasicSetting))
                {
                    node.SetElementValue("PostPerPage", !string.IsNullOrEmpty(Convert.ToString(_SettingModel.PostPerPage)) ? _SettingModel.PostPerPage : 10);
                    node.SetElementValue("IsSearchEnabled", _SettingModel.IsSearchEnabled);
                    node.SetElementValue("ThemesPath", !string.IsNullOrEmpty(_SettingModel.ThemesPath) ? _SettingModel.ThemesPath : "");
                    node.SetElementValue("CommentPerPost", !string.IsNullOrEmpty(Convert.ToString(_SettingModel.CommentPerPost)) ? _SettingModel.CommentPerPost : 5);
                    node.SetElementValue("IsCommentEnabled", _SettingModel.IsCommentEnabled);
                    node.SetElementValue("IsSocialSharingEnabled", _SettingModel.IsSocialSharingEnabled);

                }
                else if (_SettingModel.ContentType == Convert.ToInt32(ContentContexts.ContentType.EmailSetting))
                {
                    node.SetElementValue("SendingEmail", !string.IsNullOrEmpty(_SettingModel.SendingEmail) ? _SettingModel.SendingEmail : "");
                    node.SetElementValue("ReceivingEmail", !string.IsNullOrEmpty(_SettingModel.ReceivingEmail) ? _SettingModel.ReceivingEmail : "");
                }
                else if (_SettingModel.ContentType == Convert.ToInt32(ContentContexts.ContentType.ThemeSetting))
                {
                    node.SetElementValue("PagingColor", !string.IsNullOrEmpty(_SettingModel.PagingColor) ? _SettingModel.PagingColor : "");
                    node.SetElementValue("PagingActiveColor", !string.IsNullOrEmpty(_SettingModel.PagingActiveColor) ? _SettingModel.PagingActiveColor : "");
                    node.SetElementValue("PagingHoverColor", !string.IsNullOrEmpty(_SettingModel.PagingHoverColor) ? _SettingModel.PagingHoverColor : "");
                    node.SetElementValue("ButtonColor", !string.IsNullOrEmpty(_SettingModel.ButtonColor) ? _SettingModel.ButtonColor : "");
                    node.SetElementValue("NevigationBarColor", !string.IsNullOrEmpty(_SettingModel.NevigationBarColor) ? _SettingModel.NevigationBarColor : "");
                    node.SetElementValue("NevigationBarFontFamily", !string.IsNullOrEmpty(_SettingModel.NevigationBarFontFamily) ? _SettingModel.NevigationBarFontFamily : "");
                    node.SetElementValue("NevigationBarFontFamilyTxt", !string.IsNullOrEmpty(_SettingModel.NevigationBarFontFamilyTxt) ? _SettingModel.NevigationBarFontFamilyTxt : "");
                    node.SetElementValue("NevigationBarHoverColor", !string.IsNullOrEmpty(_SettingModel.NevigationBarHoverColor) ? _SettingModel.NevigationBarHoverColor : "");
                    node.SetElementValue("NevigationBarFontColor", !string.IsNullOrEmpty(_SettingModel.NevigationBarFontColor) ? _SettingModel.NevigationBarFontColor : "");
                    node.SetElementValue("NevigationBarTextFontSize", !string.IsNullOrEmpty(_SettingModel.NevigationBarTextFontSize) ? _SettingModel.NevigationBarTextFontSize : "");
                    node.SetElementValue("NevigationBarTextFontStyle", !string.IsNullOrEmpty(_SettingModel.NevigationBarTextFontStyle) ? _SettingModel.NevigationBarTextFontStyle : "");
                    node.SetElementValue("FooterColor", !string.IsNullOrEmpty(_SettingModel.FooterColor) ? _SettingModel.FooterColor : "");
                    node.SetElementValue("SidebarTitleBackgroundColor", !string.IsNullOrEmpty(_SettingModel.SidebarTitleBackgroundColor) ? _SettingModel.SidebarTitleBackgroundColor : "");
                    node.SetElementValue("SidebarTitleFontFamily", !string.IsNullOrEmpty(_SettingModel.SidebarTitleFontFamily) ? _SettingModel.SidebarTitleFontFamily : "");
                    node.SetElementValue("SidebarTitleFontFamilyTxt", !string.IsNullOrEmpty(_SettingModel.SidebarTitleFontFamilyTxt) ? _SettingModel.SidebarTitleFontFamilyTxt : "");
                    node.SetElementValue("SidebarTitleFontcolor", !string.IsNullOrEmpty(_SettingModel.SidebarTitleFontcolor) ? _SettingModel.SidebarTitleFontcolor : "");
                    node.SetElementValue("SidebarTitleFontSize", !string.IsNullOrEmpty(_SettingModel.SidebarTitleFontSize) ? _SettingModel.SidebarTitleFontSize : "");
                    //node.SetElementValue("SideBarCategoryTextFontStyle", _SettingModel.SideBarCategoryTextFontStyle);
                    node.SetElementValue("PostTitleFontColor", !string.IsNullOrEmpty(_SettingModel.PostTitleFontColor) ? _SettingModel.PostTitleFontColor : "");
                    node.SetElementValue("PostTitleFontFamily", !string.IsNullOrEmpty(_SettingModel.PostTitleFontFamily) ? _SettingModel.PostTitleFontFamily : "");
                    node.SetElementValue("PostTitleFontFamilyTxt", !string.IsNullOrEmpty(_SettingModel.PostTitleFontFamilyTxt) ? _SettingModel.PostTitleFontFamilyTxt : "");
                    //node.SetElementValue("SidebarTitleFontcolor", _SettingModel.SidebarTitleFontcolor);
                    node.SetElementValue("PostTitleFontSize", !string.IsNullOrEmpty(_SettingModel.PostTitleFontSize) ? _SettingModel.PostTitleFontSize : "");
                    //node.SetElementValue("SideBarTagTextFontStyle", _SettingModel.SideBarTagTextFontStyle);

                }
                SettingsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/Settings.xml"));
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }
        
    }
}