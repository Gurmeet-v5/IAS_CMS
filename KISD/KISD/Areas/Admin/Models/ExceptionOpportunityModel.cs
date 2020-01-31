using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class ExceptionOpportunityModel
    {
        public long ExOpportunityID { get; set; }
        public string TitleTxt { get; set; }
        public bool ExternalLinkInd { get; set; }
        public string URLTxt { get; set; }
        public string PageURLTxt { get; set; }
        public Nullable<bool> ExternalLinkTargetInd { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public string strCreateDate { get; set; }
        public string CategoryName { get; set; }
        public bool ShowOnHomeInd { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }
        public bool StatusInd { get; set; }
        public Nullable<long> ParentID { get; set; }
        public Nullable<long> SchoolCategoryID { get; set; }
        public Nullable<System.DateTime> ExOpportunityCreateDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public bool IsDeletedInd { get; set; }
        public SelectList DisplayOrderNbrSelect { get; set; }//Display Order
        public string[] SelectedRightSections { get; set; }
        public SelectList RightSections { get; set; }
        public string NameTxt { get; set; }
        public List<RightSection> ParentRightSections { get; set; }
        public List<School> Categories { get; set; }
        public List<ExceptionOpportunity> ExceptionOpportunities { get; set; }

    }


    public class ExceptionOpportunityService
    {
        private db_KISDEntities _context;
        public ExceptionOpportunityService()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// this will return list of all Videos in Video model
        /// </summary>
        /// <returns></returns>
        public List<ExceptionOpportunityModel> GetExceptionOpportunitys()
        {
            var list = new List<ExceptionOpportunityModel>();
            var TypeMasterID = Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory);
            foreach (var x in GetExceptionOpportunity())
            {
                list.Add(new ExceptionOpportunityModel
                {
                    ExOpportunityID = x.ExOpportunityID,
                    TitleTxt = x.TitleTxt,
                    AltBannerImageTxt = x.AltBannerImageTxt,
                    BannerImageID = x.BannerImageID,
                    CreateDate = x.CreateDate,
                    ExternalLinkTargetInd = x.ExternalLinkTargetInd,
                    StatusInd = x.StatusInd.Value,
                    ShowOnHomeInd = x.ShowOnHomeInd.Value,
                    DisplayOrderNbr = x.DisplayOrderNbr,
                    ExternalLinkInd = x.ExternalLinkInd.Value,
                    CategoryName = _context.Schools.Where(m => m.TypeMasterID == TypeMasterID && m.SchoolID == x.SchoolCategoryID && m.IsDeletedInd == false).Select(m => m.NameTxt).FirstOrDefault(),
                    ExOpportunityCreateDate = x.ExOpportunityCreateDate,
                    DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0"))

                });
            }
            return list;
        }

        /// <summary>
        /// IQueryable<ParrieHaynesRanch.ExceptionOpportunity> get the ExceptionOpportunity Data by passing the TypeMasterID.
        /// </summary>
        /// <returns>ExceptionOpportunityObject</returns>
        public IQueryable<ExceptionOpportunity> GetExceptionOpportunity()
        {
            return _context.ExceptionOpportunities.Where(x => (x.IsDeletedInd == false));
        }

        public SelectList GetDisplayOrder(string value)
        {
            List<DisplayOrder> status = new List<DisplayOrder>();
            var varCount = _context.ExceptionOpportunities.Where(x => x.DisplayOrderNbr != 0 && x.IsDeletedInd == false).Count();
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
        /// <returns></returns>
        public static bool ChangeExceptionOpportunityDisplayOrder(long sourceorder, long targetorder)
        {
            var _context = new db_KISDEntities();
            try
            {
                if (sourceorder < targetorder)
                {
                    var ExceptionOpportunityList = _context.ExceptionOpportunities.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.IsDeletedInd == false).ToList();
                    foreach (var item in ExceptionOpportunityList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var ExceptionOpportunityList = _context.ExceptionOpportunities.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.IsDeletedInd == false).ToList();
                    foreach (var item in ExceptionOpportunityList)
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
        /// <param name="ExOpportunityID"></param>
        public bool ChangeDeletedDisplayOrder(long sourceorder, long ExOpportunityID)
        {
            try
            {
                var _objExceptionOpportunity = _context.ExceptionOpportunities.Where(x => x.ExOpportunityID == ExOpportunityID).FirstOrDefault();
                var _obj_objMenuList = _context.ExceptionOpportunities.Where(x => x.DisplayOrderNbr > sourceorder && x.IsDeletedInd == false).ToList();
                foreach (var item in _obj_objMenuList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objExceptionOpportunity.IsDeletedInd = true;
                _objExceptionOpportunity.DisplayOrderNbr = null;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<School> GetAllHomeSchoolCategories()
        {
            List<School> lstCats = new List<School>();
            var data = _context.ExceptionOpportunities.Where(x => x.ShowOnHomeInd == true && x.StatusInd == true && x.IsDeletedInd == false).Select(x => x.SchoolCategoryID.Value).Distinct().ToList();
            if (data != null)
            {
                foreach (var d in data)
                {
                    var schoolCategory = _context.Schools.Where(x => x.SchoolID == d).FirstOrDefault();
                    lstCats.Add(schoolCategory);
                }
            }
            return lstCats;
        }
    }
}