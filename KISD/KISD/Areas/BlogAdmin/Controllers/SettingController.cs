using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class SettingController : Controller
    {
        /// <summary>
        /// Get data for Setting page
        /// </summary>
        /// <param name="SettingID"></param>
        /// <returns></returns>
        /// 
        [Authorize]
        [SessionExpire]
        public ActionResult Setting(int? SettingID, int ContentType)
        {
            var _settingContext = new Contexts.SettingContexts();
            var _emailContext = new Contexts.EmailsContexts();
            var _settingModel = new SettingModel();
            var settingType = string.Empty;
            settingType = (ContentType == 3 ? "Basic" : (ContentType == 5 ? "Theme" : (ContentType == 4 ? "Email" : "")));
            ViewBag.Title = (SettingID.HasValue ? "Edit " : "Add ") + settingType + " Setting Details ";
            ViewBag.Submit = SettingID.HasValue && SettingID.Value > 0 ? "Update" : "Save";
            bool isSearchEnabled = true; bool isCommentEnabled = true; bool isSocialSharingEnabled = true;
            string fontSize = string.Empty;
            ViewBag.FromEmails = new SelectList(_emailContext.GetEmails().Where(x => x.EmailTypeID == 1).ToList().OrderBy(x => x.EmailTxt), "EmailID", "EmailTxt");
            ViewBag.ToEmails = new SelectList(_emailContext.GetEmails().Where(x => x.EmailTypeID == 2).ToList().OrderBy(x => x.EmailTxt), "EmailID", "EmailTxt");
            _settingModel.ReceivingEmailList = GetAllReceivingEmails();
            if (SettingID.HasValue && SettingID.Value > 0)
            {
                if (_settingModel != null)
                {
                    _settingModel.ReceivingEmailList = GetAllToEmails();//Receiving Emails
                    var array_ReceivingEmailID = new string[] { };
                    var receiveEmailTxt = _settingContext.GetSettings().Select(x => x.ReceivingEmail).FirstOrDefault();
                    _settingModel.SendingEmail = _settingContext.GetSettings().Select(x => x.SendingEmail).FirstOrDefault();
                    if (receiveEmailTxt.Contains(','))
                    {
                        array_ReceivingEmailID = _settingContext.GetSettings().Select(x => x.ReceivingEmail).FirstOrDefault().Split(',');
                    }
                    else
                    {
                        array_ReceivingEmailID = _settingContext.GetSettings().Select(x => x.ReceivingEmail).ToArray();
                    }

                    var selecetdReceivingEmailsarray = Array.ConvertAll<int, string>(_emailContext.GetEmails().Where(x => array_ReceivingEmailID.Contains(x.EmailID.ToString())).Select(x => x.EmailID).ToArray(),
                                                                   delegate (int i)
                                                                   {
                                                                       return (string)i.ToString();
                                                                   });

                    isSearchEnabled = _settingContext.GetSettings().Select(x => x.IsSearchEnabled).FirstOrDefault();
                    isCommentEnabled = _settingContext.GetSettings().Select(x => x.IsCommentEnabled).FirstOrDefault();
                    isSocialSharingEnabled = _settingContext.GetSettings().Select(x => x.IsSocialSharingEnabled).FirstOrDefault();
                    _settingModel.SelectedReceivingEmail = selecetdReceivingEmailsarray;
                }
            }
            else
            {
                ViewBag.Submit = "Save";
                string[] ToEmailarr = new string[] { "0" };
                _settingModel.SelectedToEmails = ToEmailarr;
            }
            #region Fill Values
            var settinglist = _settingContext.GetSettings().FirstOrDefault();
            _settingModel.PagingColor = !string.IsNullOrEmpty(settinglist.PagingColor) ? settinglist.PagingColor : "";
            _settingModel.PagingActiveColor = !string.IsNullOrEmpty(settinglist.PagingActiveColor) ? settinglist.PagingActiveColor : "";
            _settingModel.PagingHoverColor = !string.IsNullOrEmpty(settinglist.PagingHoverColor) ? settinglist.PagingHoverColor : "";
            _settingModel.NevigationBarColor = !string.IsNullOrEmpty(settinglist.NevigationBarColor) ? settinglist.NevigationBarColor : "";
            _settingModel.NevigationBarTextFontSize = !string.IsNullOrEmpty(settinglist.NevigationBarTextFontSize) ? settinglist.NevigationBarTextFontSize : "";
            _settingModel.NevigationBarFontColor = !string.IsNullOrEmpty(settinglist.NevigationBarFontColor) ? settinglist.NevigationBarFontColor : "";
            _settingModel.NevigationBarHoverColor = !string.IsNullOrEmpty(settinglist.NevigationBarHoverColor) ? settinglist.NevigationBarHoverColor : "";
            _settingModel.FooterColor = !string.IsNullOrEmpty(settinglist.FooterColor) ? settinglist.FooterColor : "";
            _settingModel.SidebarTitleBackgroundColor = !string.IsNullOrEmpty(settinglist.SidebarTitleBackgroundColor) ? settinglist.SidebarTitleBackgroundColor : "";
            _settingModel.SidebarTitleFontcolor = !string.IsNullOrEmpty(settinglist.SidebarTitleFontcolor) ? settinglist.SidebarTitleFontcolor : "";
            _settingModel.SidebarTitleFontSize = !string.IsNullOrEmpty(settinglist.SidebarTitleFontSize) ? settinglist.SidebarTitleFontSize : "";
            _settingModel.PostTitleFontColor = !string.IsNullOrEmpty(settinglist.PostTitleFontColor) ? settinglist.PostTitleFontColor : "";
            _settingModel.PostTitleFontSize = !string.IsNullOrEmpty(settinglist.PostTitleFontSize) ? settinglist.PostTitleFontSize : "";
            _settingModel.ButtonColor = !string.IsNullOrEmpty(settinglist.ButtonColor) ? settinglist.ButtonColor : "";
            _settingModel.ContentType = ContentType;
            _settingModel.IsSearchEnabled = isSearchEnabled;
            _settingModel.IsCommentEnabled = isCommentEnabled;
            _settingModel.IsSocialSharingEnabled = isSocialSharingEnabled;
            ViewBag.NevigationbarFontFamily = GetAllFont(_settingModel != null ? _settingContext.GetSettings().Select(x => x.NevigationBarFontFamily).FirstOrDefault() : string.Empty);// new SelectList(font,"FontID","Name");
            ViewBag.SidebarTitleFontFamily = GetAllFont(_settingModel != null ? _settingContext.GetSettings().Select(x => x.SidebarTitleFontFamily).FirstOrDefault() : string.Empty);
            ViewBag.PostTitleFontFamily = GetAllFont(_settingModel != null ? _settingContext.GetSettings().Select(x => x.PostTitleFontFamily).FirstOrDefault() : string.Empty);
            ViewBag.IsSearchEnabled = Models.Common.GetStatusListBoolean(isSearchEnabled ? "true" : "false");
            ViewBag.IsCommentEnabled = Models.Common.GetStatusListBoolean(isCommentEnabled ? "true" : "false");
            ViewBag.IsSocialSharingEnabled = Models.Common.GetStatusListBoolean(isSocialSharingEnabled ? "true" : "false");
            ViewBag.fontSize = Models.Common.GetFontList("12");
            _settingModel.PostPerPage = _settingContext.GetSettings().Select(x => x.PostPerPage).FirstOrDefault();
            _settingModel.CommentPerPost = _settingContext.GetSettings().Select(x => x.CommentPerPost).FirstOrDefault();
            #endregion

            return View(_settingModel);
        }

        private SelectList GetAllReceivingEmails()
        {

            var _SettingContexts = new Contexts.SettingContexts();
            var _EmailsContexts = new Contexts.EmailsContexts();
            var receivingEmail = _SettingContexts.GetSettings().Select(x => x.ReceivingEmail).FirstOrDefault().ToArray();
            var list = new List<ReceivingEmailModel>();
            foreach (var item in receivingEmail)
            {
                list = _EmailsContexts.GetEmails().Select(m => new ReceivingEmailModel()
                {
                    ReceivingEmailID = m.EmailID,
                    ReceivingEmailTxt = m.EmailTxt
                }).ToList();
            }

            var objSocialMedia = new ReceivingEmailModel();
            objSocialMedia.ReceivingEmailTxt = "-- Select Receiving Emails --";
            objSocialMedia.ReceivingEmailID = 0;
            list.Insert(0, objSocialMedia);
            var objselectlist = new SelectList(list, "ReceivingEmailID", "ReceivingEmailTxt");
            return objselectlist;
        }

        /// <summary>
        /// Method to save/ update the added rentals into the database.
        /// </summary>
        /// <param name="_Settingmodel">SettingModel that will contain the values saved in the Setting View</param>
        /// <param name="command">This will contain value on click of Cancel button</param>
        /// <param name="fm">It contains the form collection (hidden field etc) values.</param>
        /// <returns></returns>
        [HttpPost]
        //[Authorize]
        //[SessionExpire]
        [ValidateInput(false)]
        //[AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Setting(SettingModel _Settingmodel, string command, FormCollection fm, int? SettingType)
        {
            var SettingContext = new Contexts.SettingContexts();
            var settingType = string.Empty;
            settingType = (_Settingmodel.ContentType == 3 ? "Basic" : (_Settingmodel.ContentType == 5 ? "Theme" : (_Settingmodel.ContentType == 4 ? "Email" : "")));
            ViewBag.Title = (_Settingmodel.SettingID > 0 ? "Edit " : "Add ") + settingType + " Setting";
            ViewBag.Submit = _Settingmodel.SettingID > 0 ? "Update" : "Save";
            if (string.IsNullOrEmpty(command))
            {
                try
                {
                    _Settingmodel.NevigationBarFontFamilyTxt = fm["hidNevigationbarFontFamily"] == null ? "Arial" : fm["hidNevigationbarFontFamily"].ToString();
                    _Settingmodel.SidebarTitleFontFamilyTxt = fm["hidSidebarTitleFontFamily"] == null ? "Arial" : fm["hidSidebarTitleFontFamily"].ToString();
                    _Settingmodel.PostTitleFontFamilyTxt = fm["hidPostTitleFontFamily"] == null ? "Arial" : fm["hidPostTitleFontFamily"].ToString();

                    if (ViewBag.Submit == "Save")
                    {
                        var str = string.Empty;
                        foreach (var item in _Settingmodel.SelectedReceivingEmail)
                        {
                            str = str + "," + item.ToString();
                        }
                        _Settingmodel.ReceivingEmail = str.TrimEnd(',', ' ').TrimStart(',', ' ');
                        SettingContext.AddSetting(_Settingmodel);
                        TempData["AlertMessage"] = settingType + " setting saved successfully.";
                    }
                    else
                    {
                        var str = string.Empty;
                        if (_Settingmodel.SelectedReceivingEmail != null)
                        {
                            foreach (var item in _Settingmodel.SelectedReceivingEmail)
                            {
                                str = str + "," + item.ToString();
                            }
                            _Settingmodel.ReceivingEmail = str.TrimEnd(',', ' ').TrimStart(',', ' ');
                        }

                        SettingContext.EditSetting(_Settingmodel);
                        TempData["AlertMessage"] = settingType + " setting updated successfully.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                }
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Get all to emails
        /// </summary>
        /// <returns></returns>
        private SelectList GetAllToEmails()
        {
            var _emailContext = new Contexts.EmailsContexts();
            var list = _emailContext.GetEmails().Where(x => x.EmailTypeID == 2).OrderBy(x => x.EmailTxt).Select(x => new EmailsModel()
            {
                EmailID = x.EmailID,
                EmailTxt = x.EmailTxt
            }).ToList();
            var objEmail = new EmailsModel();
            objEmail.EmailTxt = "-- Select To Email --";
            objEmail.EmailID = 0;
            list.Insert(0, objEmail);
            var objselectlist = new SelectList(list, "EmailID", "EmailTxt");
            return objselectlist;
        }
        /// <summary>
        /// Get All Fonts list
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private List<SelectListItem> GetAllFont(string value)
        {
            var items = new List<SelectListItem>();
            DataTable dt = new DataTable();
            dt.Columns.Add("FontID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            var dr1 = new SelectListItem();
            dr1.Text = "-- Select Font --";
            dr1.Value = (0).ToString(); ;
            items.Add(dr1);

            var font = new[] { "Arial", "Times New Roman", "Open Sans", "Josefin Slab", "Lato", "PT Sans", "Droid Serif", "Raleway", "Oswald", "PT Sans Narrow" };

            for (int i = 0; i < font.Count(); i++)
            {
                var data = new SelectListItem();
                data.Text = font[i];
                data.Value = (i + 1).ToString(); ;
                items.Add(data);
            }
            items.Where(x => x.Value == (!string.IsNullOrEmpty(value) ? value : "0")).FirstOrDefault().Selected = true;
            return items;
        }
    }
}
