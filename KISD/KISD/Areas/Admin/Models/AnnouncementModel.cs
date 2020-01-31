using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class AnnouncementModel
    {
        public long AnnouncementID { get; set; }
        public string TitleTxt { get; set; }
        public string ImageURLTxt { get; set; }
        public string AltImageTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public Nullable<System.DateTime> ScheduleDateTime { get; set; }
        public Nullable<System.DateTime> DisplayStartDate { get; set; }
        public Nullable<System.DateTime> DisplayEndDate { get; set; }
        public bool StatusInd { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public Nullable<System.DateTime> AnnouncementCreateDate { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [RegularExpression(@"\d{1,2}/\d{1,2}/\d{4,4}", ErrorMessage = "Invalid date.")]
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public Nullable<long> TypeMasterID { get; set; }
        public string ScheduleTimeTxt { get; set; }
        public SelectList DisplayOrderNbrSelect { get; set; }//Display Order
    }
    public class AnnouncementService
    {
        private db_KISDEntities _context;
        public AnnouncementService()
        {
            _context = new db_KISDEntities();
        }


        /// <summary>
        /// this will return list of all Videos in Video model
        /// </summary>
        /// <returns></returns>
        public List<AnnouncementModel> GetAnnouncements(long TypeMasterID)
        {
            var list = new List<AnnouncementModel>();
            foreach (var x in GetAnnouncement(TypeMasterID))
            {
                list.Add(new AnnouncementModel
                {
                    AnnouncementID = x.AnnouncementID,
                    TitleTxt = x.TitleTxt,
                    AltImageTxt = x.AltImageTxt,
                    ImageURLTxt = x.ImageURLTxt,
                    CreateDate = x.CreateDate,
                    TypeMasterID = x.TypeMasterID,
                    StatusInd = x.StatusInd.Value,
                    DisplayStartDate = x.DisplayStartDate,
                    DisplayEndDate = x.DisplayEndDate,
                    DisplayOrderNbr = x.DisplayOrderNbr,
                    AnnouncementCreateDate = x.AnnouncementCreateDate,
                    DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0"), x.TypeMasterID.Value)

                });
            }
            return list;
        }

        /// <summary>
        /// IQueryable<ParrieHaynesRanch.Announcement> get the Announcement Data by passing the TypeMasterID.
        /// </summary>
        /// <param name="TypeMasterID">TypeMasterID</param>
        /// <returns>AnnouncementObject</returns>
        public IQueryable<Announcement> GetAnnouncement(long? TypeMasterID)
        {
            return _context.Announcements.Where(x => (x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false));
        }

        /// <summary>
        /// Get all Announcement of defined type
        /// </summary>
        /// <param name="AnnouncementType"></param>
        /// <returns></returns>
        public string GetAnnouncementType(long AnnouncementType)
        {
            return _context.TypeMasters.Where(x => x.TypeMasterID == AnnouncementType).Select(x => x.TypeMasterName).FirstOrDefault();
        }

        public SelectList GetDisplayOrder(string value, long TypeMasterID)
        {
            List<DisplayOrder> status = new List<DisplayOrder>();
            var varCount = _context.Announcements.Where(x => x.DisplayOrderNbr != 0 && x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).Count();
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
        public static bool ChangeAnnouncementDisplayOrder(long sourceorder, long targetorder, long ContentID, int TypeMasterID)
        {
            var _context = new db_KISDEntities();
            try
            {
                if (sourceorder < targetorder)
                {
                    var AnnouncementList = _context.Announcements.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).ToList();
                    foreach (var item in AnnouncementList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var AnnouncementList = _context.Announcements.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).ToList();
                    foreach (var item in AnnouncementList)
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
        /// <param name="AnnouncementID"></param>
        /// <param name="TypeMasterID"></param>
        public bool ChangeDeletedDisplayOrder(long sourceorder, long AnnouncementID, long TypeMasterID)
        {
            try
            {
                var _objAnnouncement = _context.Announcements.Where(x => x.AnnouncementID == AnnouncementID).FirstOrDefault();
                var _objAnnouncementsList = _context.Announcements.Where(x => x.DisplayOrderNbr > sourceorder && x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).ToList();
                foreach (var item in _objAnnouncementsList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objAnnouncement.DisplayOrderNbr = null;
                _objAnnouncement.IsDeletedInd = true;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}