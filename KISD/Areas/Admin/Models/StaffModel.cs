using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class StaffModel
    {
        public long StaffID { get; set; }
        public string FirstNameTxt { get; set; }
        public string LastNameTxt { get; set; }
        public string NameTxt { get; set; }
        
        public string DesignationTxt { get; set; }
        public long SchoolCategoryID { get; set; }
        public long SchoolID { get; set; }
        public string SchoolCategoryName { get; set; }
        public string SchoolName { get; set; }
        public string PhoneTxt { get; set; }
        public string EmailTxt { get; set; }
        public string DepartmentsName { get; set; }
        public string SubDepartmentsName { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public bool StatusInd { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public Nullable<DateTime> StaffCreateDate { get; set; }
        public string strStaffCreateDate { get; set; }
        public DateTime CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public SelectList DisplayOrderNbrSelect { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public List<SelectListItem> DepartmentsList { get; set; }
        public List<SelectListItem> SubDepartmentsList { get; set; }
        public List<SelectListItem> SchoolCategoryList { get; set; }
        public List<SelectListItem> SchoolList { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string[] SelectedDepartment { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string[] SelectedSubDepartment { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string[] SelectedSchoolCategory { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string[] SelectedSchool { get; set; }
        public IPagedList<Staff> StaffResult { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }
        public string AbstractTxt { get; set; }
        public bool IsPagingVisible { get; set; }
    }

    public class StaffModelService
    {
        private db_KISDEntities _context;
        public StaffModelService()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// this will return list of all Videos in Video model
        /// </summary>
        /// <returns></returns>
        public List<StaffModel> GetStaff()
        {
            var list = new List<StaffModel>();
            foreach (var x in GetAllStaff())
            {
                list.Add(new StaffModel
                {
                    StaffID = x.StaffID,
                    DesignationTxt = x.DesignationTxt,
                    StaffCreateDate = x.StaffCreateDate.HasValue ? x.StaffCreateDate.Value : DateTime.Now,
                    CreateDate = x.CreateDate.HasValue ? x.CreateDate.Value : DateTime.Now,
                    StatusInd = x.StatusInd.HasValue ? x.StatusInd.Value : false,
                    DisplayOrderNbr = x.DisplayOrderNbr,
                    EmailTxt = x.EmailTxt,
                    CreateByID = x.CreateByID,
                    IsDeletedInd = x.IsDeletedInd,
                    LastModifyByID = x.LastModifyByID,
                    LastModifyDate = x.LastModifyDate.HasValue ? x.LastModifyDate.Value : DateTime.Now,
                    FirstNameTxt = x.FirstNameTxt,
                    LastNameTxt = x.LastNameTxt,
                    NameTxt = x.FirstNameTxt + " " + x.LastNameTxt,
                    PhoneTxt = x.PhoneTxt,
                    strStaffCreateDate = x.StaffCreateDate.HasValue ? x.StaffCreateDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString(),
                    DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0")),
                    DepartmentsName = GetDepartmentsName(x.StaffID, true, false),

                });
            }
            return list;
        }

        public IQueryable<Staff> GetAllStaff()
        {
            return _context.Staffs.Where(x => x.IsDeletedInd == false);
        }

        public SelectList GetDisplayOrder(string value)
        {
            List<DisplayOrder> status = new List<DisplayOrder>();
            var varCount = _context.Staffs.Where(x => x.DisplayOrderNbr != 0 && x.IsDeletedInd == false).Count();
            for (var i = 1; i <= varCount; i++)
            {
                status.Add(new DisplayOrder { DisplayID = i, DisplayOrderNbr = i.ToString() });
            }
            SelectList objinfo = new SelectList(status, "DisplayID", "DisplayOrderNbr", value);
            return objinfo;
        }

        /// <summary>
        /// Change display order 
        /// </summary>
        /// <param name="sourceorder"></param>
        /// <param name="targetorder"></param>
        /// <param name="ContentID"></param>
        /// <returns></returns>
        public static bool ChangeDisplayOrder(long sourceorder, long targetorder)
        {
            var _context = new db_KISDEntities();
            try
            {
                if (sourceorder < targetorder)
                {
                    var List = _context.Staffs.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.IsDeletedInd == false).ToList();
                    foreach (var item in List)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var List = _context.Staffs.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.IsDeletedInd == false).ToList();
                    foreach (var item in List)
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

        public bool ChangeDeletedDisplayOrder(long sourceorder, long StaffID)
        {
            try
            {
                var objstaff = _context.Staffs.Where(x => x.StaffID == StaffID).FirstOrDefault();
                var _obj_objList = _context.Staffs.Where(x => x.DisplayOrderNbr > sourceorder && x.IsDeletedInd == false).ToList();
                foreach (var item in _obj_objList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                objstaff.DisplayOrderNbr = null;
                objstaff.IsDeletedInd = true;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetDepartmentsName(long StaffID, bool onlyParentDepts, bool onlyChildDepts)
        {
            string DepartmentsName = string.Empty;
            var records = _context.DepartmentStaffs.Where(x => x.StaffID == StaffID).ToList();
            if (records != null)
            {
                if (onlyParentDepts && !onlyChildDepts)
                {
                    foreach (var s in records)
                    {
                        if (_context.Departments.Where(x => x.DepartmentID == s.DepartmentID && x.ParentID == null).Select(x => x.NameTxt).FirstOrDefault() != null)
                            DepartmentsName += _context.Departments.Where(x => x.DepartmentID == s.DepartmentID && x.ParentID == null).Select(x => x.NameTxt).FirstOrDefault() + ",";
                    }
                }

                if (onlyChildDepts && !onlyParentDepts)
                {
                    foreach (var s in records)
                    {
                        if (_context.Departments.Where(x => x.DepartmentID == s.DepartmentID && x.ParentID != null).Select(x => x.NameTxt).FirstOrDefault() != null)
                            DepartmentsName += _context.Departments.Where(x => x.DepartmentID == s.DepartmentID && x.ParentID != null).Select(x => x.NameTxt).FirstOrDefault() + ",";
                    }
                }

                if (onlyParentDepts && onlyChildDepts)
                {
                    foreach (var s in records)
                    {
                        if (_context.Departments.Where(x => x.DepartmentID == s.DepartmentID).Select(x => x.NameTxt).FirstOrDefault() != null)
                            DepartmentsName += _context.Departments.Where(x => x.DepartmentID == s.DepartmentID).Select(x => x.NameTxt).FirstOrDefault() + ",";
                    }
                }

            }

            DepartmentsName = DepartmentsName.Substring(0, DepartmentsName.Length - 1);
            return DepartmentsName;
        }

        public string GetSchoolData(long SchoolID)
        {
            var records = _context.Schools.Where(x => x.SchoolID == SchoolID).Select(x=>x.NameTxt).FirstOrDefault();
            return records;
        }
    }
}