using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Models
{
    public class DepartmentModel
    {
        public long DepartmentID { get; set; }
        public string NameTxt { get; set; }
        public string URLTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public string AddressTxt { get; set; }
        public string strCreateDate { get; set; }
        
        public string PhoneNumberTxt { get; set; }
        public string FaxNumberTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public Nullable<long> ParentID { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public bool StatusInd { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescription { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public Nullable<System.DateTime> DepartmentCreateDate { get; set; }
        public SelectList DisplayOrderNbrSelect { get; set; }//Display Order
        public SelectList StatusNumSelect { get; set; }//Status
        public string[] SelectedRightSections { get; set; }

        public SelectList RightSections { get; set; }
        public List<Department> SubDepartmentListing { get; set; }
        public List<RightSection> ParentRightSections { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
    }
    //Display Order 
    public class DepartmentDisplayOrder
    {
        public int DisplayID { get; set; }
        public string DisplayOrderNbr { get; set; }
    }
    public class DepartmentService
    {
        private db_KISDEntities _context;
        public DepartmentService()
        {
            _context = new db_KISDEntities();
        }
        /// <summary>
        /// Get all Departments of defined type
        /// </summary>
        /// <param name="EmailType"></param>
        /// <returns></returns>
        public IQueryable<Department> GetAllDepartments()
        {
            return _context.Departments.Where(x =>  x.ParentID == null && (x.IsDeletedInd == false || x.IsDeletedInd == null));
        }

        public List<DepartmentModel> GetDepartments()
        {
            var list = new List<DepartmentModel>();
            foreach (var item in GetAllDepartments())
            {
                list.Add(new DepartmentModel
                {
                    DepartmentID = item.DepartmentID,
                    CreateDate = item.CreateDate,
                    DepartmentCreateDate = item.DepartmentCreateDate,
                    DescriptionTxt = item.DescriptionTxt,
                    StatusInd = item.StatusInd.Value,
                    NameTxt = item.NameTxt,
                    PhoneNumberTxt=item.PhoneNumberTxt,
                    PageMetaDescription = item.PageMetaDescription,
                    PageMetaTitleTxt = item.PageMetaTitleTxt,
                    URLTxt = item.URLTxt,
                    DisplayOrderNbr = string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()) ? 0 : item.DisplayOrderNbr.Value,
                    DisplayOrderNbrSelect = GetDisplayOrder(item.DisplayOrderNbr.ToString()),
                    StatusNumSelect = GetStatus(item.StatusInd.ToString())
                });
            }
            return list;
        }

        public IQueryable<Department> GetAllSubDepartments( int ParentId)
        {
            return _context.Departments.Where(x => x.ParentID == ParentId && (x.IsDeletedInd == false || x.IsDeletedInd == null));
        }

        public List<DepartmentModel> GetSubDepartments( int ParentId)
        {
            var list = new List<DepartmentModel>();
            foreach (var item in GetAllSubDepartments(ParentId))
            {
                list.Add(new DepartmentModel
                {
                    DepartmentID = item.DepartmentID,
                    AddressTxt = item.AddressTxt,
                    CreateDate = item.CreateDate,
                    DepartmentCreateDate = item.DepartmentCreateDate,
                    DescriptionTxt = item.DescriptionTxt,                
                    StatusInd = item.StatusInd.Value,                   
                    NameTxt = item.NameTxt,
                    PhoneNumberTxt = item.PhoneNumberTxt,
                    PageMetaDescription = item.PageMetaDescription,
                    PageMetaTitleTxt = item.PageMetaTitleTxt,
                    URLTxt = item.URLTxt,
                    DisplayOrderNbr = string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()) ? 0 : item.DisplayOrderNbr.Value,
                    DisplayOrderNbrSelect = GetDisplayOrderSubDepartment(item.DisplayOrderNbr.ToString(), ParentId),
                    StatusNumSelect = GetStatus(item.StatusInd.ToString())
                });
            }
            return list;
        }

        //Display Order 
        public SelectList GetDisplayOrder(string value)
        {
            List<DepartmentDisplayOrder> status = new List<DepartmentDisplayOrder>();
            var varCount = _context.Departments.Where(x => x.DisplayOrderNbr != 0 && x.ParentID == null && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
            for (var i = 1; i <= varCount; i++)
            {
                status.Add(new DepartmentDisplayOrder { DisplayID = i, DisplayOrderNbr = i.ToString() });
            }
            SelectList objinfo = new SelectList(status, "DisplayID", "DisplayOrderNbr", value);
            return objinfo;
        }

        public SelectList GetStatus(string value)
        {
            Dictionary<string, string> StatusTypes = new Dictionary<string, string>();
            StatusTypes.Add("Active", "1");
            StatusTypes.Add("InActive", "0");

            List<SelectListItem> items = new List<SelectListItem>();
            var selectedStatus = _context.Departments.Select(x => x.StatusInd).FirstOrDefault();
            bool IsSelected = false;
            foreach (KeyValuePair<string, string> stat in StatusTypes)
            {
                if (stat.Value == selectedStatus.ToString()) { IsSelected = true; }
                items.Add(new SelectListItem { Text = stat.Key, Value = stat.Value, Selected = IsSelected });
            }
            SelectList objinfo = new SelectList(items, "Value", "Text", value);
            return objinfo;
        }

        //Display Order Sub Department
        public SelectList GetDisplayOrderSubDepartment(string value, int parentID)
        {
            List<DepartmentDisplayOrder> status = new List<DepartmentDisplayOrder>();
            var varCount = _context.Departments.Where(x => x.DisplayOrderNbr != 0 && x.ParentID == parentID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Count();
            for (var i = 1; i <= varCount; i++)
            {
                status.Add(new DepartmentDisplayOrder { DisplayID = i, DisplayOrderNbr = i.ToString() });
            }
            SelectList objinfo = new SelectList(status, "DisplayID", "DisplayOrderNbr", value);
            return objinfo;
        }

        /// <summary>
        /// Change display order 
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="targetorder"></param>
        /// <param name="DepartmentID"></param>
        /// <returns></returns>
        public bool ChangeImageDisplayOrder(long sourceorder, long targetorder, long DepartmentID)
        {
            try
            {
                if (sourceorder < targetorder)
                {
                    var DepartmentList = _context.Departments.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.ParentID == null && (x.IsDeletedInd == false || x.IsDeletedInd == null)).ToList();
                    foreach (var item in DepartmentList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var DepartmentList = _context.Departments.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.ParentID == null).ToList();
                    foreach (var item in DepartmentList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr += 1;
                    }
                }
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Change display order of Sub Department
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="targetorder"></param>
        /// <param name="DepartmentID"></param>
        /// <param name="ParentID"></param>
        /// <returns></returns>
        public bool ChangeImageDisplayOrderSubDepartment(long sourceorder, long targetorder, long DepartmentID, int ParentID)
        {
            try
            {
                if (sourceorder < targetorder)
                {
                    var DepartmentList = _context.Departments.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.ParentID == ParentID).ToList();
                    foreach (var item in DepartmentList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var DepartmentList = _context.Departments.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.ParentID == ParentID).ToList();
                    foreach (var item in DepartmentList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr += 1;
                    }
                }
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Change Display order of all.
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="DepartmentID"></param>
        public bool ChangeDeletedDisplayOrder(long sourceorder, long DepartmentID)
        {
            try
            {
                var _objDepartment = _context.Departments.Where(x => x.DepartmentID == DepartmentID).FirstOrDefault();
                var _objDepartmentList = _context.Departments.Where(x => x.DisplayOrderNbr > sourceorder && x.ParentID == null && x.IsDeletedInd==false).ToList();
                foreach (var item in _objDepartmentList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objDepartment.DisplayOrderNbr = null;
                _objDepartment.IsDeletedInd = true;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Change Display order of all Sub Department.
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="DepartmentID"></param>
        /// <param name="ParentID"></param>
        public bool ChangeDeletedDisplayOrderSubDepartment(long sourceorder, long DepartmentID, int ParentID)
        {
            try
            {
                var _objDepartment = _context.Departments.Where(x => x.DepartmentID == DepartmentID).FirstOrDefault();
                var _objDepartmentList = _context.Departments.Where(x => x.DisplayOrderNbr > sourceorder && x.ParentID == ParentID).ToList();
                foreach (var item in _objDepartmentList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                //_context.Departments.Remove(_objDepartment);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public List<ContentModel> GetDepartmentContent(int contentTypeID, long UserID, long DepartmentID)
        {
            var list = new List<ContentModel>();
            foreach (var item in GetAllDepartment(contentTypeID, UserID).Where(x=>x.DepartmentID == DepartmentID))
            {
                list.Add(new ContentModel
                {
                    ContentID = item.ContentID,
                    ContentTypeID = item.ContentTypeID,
                    AbstractTxt = item.AbstractTxt,
                    CreateDate = item.CreateDate,
                    DescriptionTxt = item.DescriptionTxt,
                    ExternalLinkTargetInd = item.ExternalLinkTargetInd,
                    ExternalLinkTxt = item.ExternalLinkTxt,
                    BannerImageID = item.BannerImageID,
                    StatusInd = item.StatusInd,
                    IsExternalLinkInd = item.IsExternalLinkInd,
                    MenuTitleTxt = item.MenuTitleTxt,
                    PageMetaDescriptionTxt = item.PageMetaDescriptionTxt,
                    PageMetaTitleTxt = item.PageMetaTitleTxt,
                    PageTitleTxt = item.PageTitleTxt,
                    AltBannerImageTxt = item.AltBannerImageTxt,
                    PageURLTxt = item.PageURLTxt,
                    DisplayOrderNbr = string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()) ? 0 : item.DisplayOrderNbr.Value,
                    DisplayOrderNbrSelect = GetDisplayOrder(item.DisplayOrderNbr.ToString(), contentTypeID, DepartmentID),
                    StatusNumSelect = GetStatus(item.StatusInd.ToString(), contentTypeID, DepartmentID),
                    ContentCreateDate = item.ContentCreateDate
                });
            }
            return list;
        }
        public IQueryable<Content> GetAllDepartment(int ContentTypeId, long UserID)
        {
            var RoleID = _context.UserRoles.Where(x => x.UserID == UserID).Select(x => x.RoleID).FirstOrDefault();
            if (ContentTypeId == Convert.ToInt32(ContentType.Fly) && RoleID != Convert.ToInt32(UserType.SuperAdmin)
                    && RoleID != Convert.ToInt32(UserType.Admin))
            {
                var AllFlyPages = _context.Contents.Where(z => z.ContentTypeID == ContentTypeId && z.ParentID == null
                && (z.IsDeletedInd == false || z.IsDeletedInd == null)).ToList();

                var FlyPagesCreatedByUser = _context.Contents.Where(z => z.ContentTypeID == ContentTypeId && z.ParentID == null
                && (z.IsDeletedInd == false || z.IsDeletedInd == null)
                && (z.CreateByID == UserID || z.LastModifyByID == UserID)).ToList();

                var AllDeptList = _context.Departments.Where(x => x.IsDeletedInd == false && x.StatusInd == true).Select(x => x.DescriptionTxt).ToArray();

                var result = AllFlyPages.Where(x => AllDeptList.Contains(x.PageURLTxt)).Distinct().ToList();
                return FlyPagesCreatedByUser.Union(result).AsQueryable();

                //return _context.Contents.Where(x => x.ContentTypeID == ContentTypeId && x.ParentID == null
                //&& (x.IsDeletedInd == false || x.IsDeletedInd == null)
                //&& (x.CreateByID == UserID || x.LastModifyByID == UserID));
            }
            else
                return _context.Contents.Where(x => x.ContentTypeID == ContentTypeId && x.ParentID == null
                && (x.IsDeletedInd == false || x.IsDeletedInd == null));
        }
        public SelectList GetStatus(string value, int contentTypeID, long DepartmentID)
        {
            Dictionary<string, string> StatusTypes = new Dictionary<string, string>();
            StatusTypes.Add("Active", "1");
            StatusTypes.Add("InActive", "0");

            List<SelectListItem> items = new List<SelectListItem>();
            var selectedStatus = _context.Contents.Where(x => x.ContentTypeID == contentTypeID && x.DepartmentID == DepartmentID).Select(x => x.StatusInd).FirstOrDefault();
            bool IsSelected = false;
            foreach (KeyValuePair<string, string> stat in StatusTypes)
            {
                if (stat.Value == selectedStatus.ToString()) { IsSelected = true; }
                items.Add(new SelectListItem { Text = stat.Key, Value = stat.Value, Selected = IsSelected });
            }
            SelectList objinfo = new SelectList(items, "Value", "Text", value);
            return objinfo;
        }
        //Display Order 
        public SelectList GetDisplayOrder(string value, int contentTypeID, long DepartmentID)
        {
            List<MenuDisplayOrder> status = new List<MenuDisplayOrder>();
            var varCount = _context.Contents.Where(x => x.DisplayOrderNbr != 0 && x.ContentTypeID == contentTypeID && x.ParentID == null && (x.IsDeletedInd == false || x.IsDeletedInd == null) && x.DepartmentID == DepartmentID).Count();
            for (var i = 1; i <= varCount; i++)
            {
                status.Add(new MenuDisplayOrder { DisplayID = i, DisplayOrderNbr = i.ToString() });
            }
            SelectList objinfo = new SelectList(status, "DisplayID", "DisplayOrderNbr", value);
            return objinfo;
        }
    }
}