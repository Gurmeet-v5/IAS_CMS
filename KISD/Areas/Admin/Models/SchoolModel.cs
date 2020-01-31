using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class SchoolModel
    {
        public long SchoolID { get; set; }
        public long SchoolCategoryID { get; set; }
        public string SchoolCategoryName { get; set; }
        public string NameTxt { get; set; }
        public string AddressTxt { get; set; }
        public string PhoneNumberTxt { get; set; }
        public string WebsiteURLTxt { get; set; }
        public bool StatusInd { get; set; }
        public DateTime SchoolCreateDate { get; set; }
        public DateTime CreateDate { get; set; }
        public string strCreateDate { get; set; }
        public long CreatedByID { get; set; }
        public DateTime LastModifyDate { get; set; }
        public long LastModifyByID { get; set; }
        public bool IsDeletedInd { get; set; }
        public long TypeMasterID { get; set; }
        public SelectList StatusNumSelect { get; set; }
        public string PageURLTxt { get; set; }
    }

    public class ModelService
    {
        private db_KISDEntities _context;
        public ModelService()
        {
            _context = new db_KISDEntities();
        }
        public List<SchoolModel> GetData(long tmi)
        {
            var list = new List<SchoolModel>();
            foreach (var item in GetAllData(tmi))
            {
                list.Add(new SchoolModel
                {
                    SchoolID = item.SchoolID,
                    AddressTxt = item.AddressTxt,
                    CreatedByID = item.CreateByID.HasValue ? item.CreateByID.Value : 0,
                    CreateDate = item.CreateDate.HasValue ? item.CreateDate.Value : DateTime.Now,
                    LastModifyByID = item.LastModifyByID.HasValue ? item.LastModifyByID.Value : 0,
                    LastModifyDate = item.LastModifyDate.HasValue ? item.LastModifyDate.Value : DateTime.Now,
                    NameTxt = item.NameTxt,
                    PhoneNumberTxt = item.PhoneNumberTxt,
                    StatusInd = item.StatusInd.HasValue ? item.StatusInd.Value : false,
                    SchoolCategoryID = item.SchoolCategoryID.HasValue ? item.SchoolCategoryID.Value : 0,
                    SchoolCreateDate = item.SchoolCreateDate.HasValue ? item.SchoolCreateDate.Value : DateTime.Now,
                    TypeMasterID = item.TypeMasterID.HasValue ? item.TypeMasterID.Value : 0,
                    WebsiteURLTxt = item.WebsiteURLTxt,
                    SchoolCategoryName = GetCategoryName(item.SchoolCategoryID.HasValue ? item.SchoolCategoryID.Value : 0),
                    PageURLTxt = item.PageURLTxt
                });
            }
            return list;
        }

        public IQueryable<School> GetAllData(long tmi)
        {
            return _context.Schools.Where(x => x.TypeMasterID == tmi && x.IsDeletedInd == false);
        }

        public SelectList GetStatus(string value, int contentTypeID)
        {
            Dictionary<string, string> StatusTypes = new Dictionary<string, string>();
            StatusTypes.Add("Active", "1");
            StatusTypes.Add("InActive", "0");

            List<SelectListItem> items = new List<SelectListItem>();
            var selectedStatus = _context.Contents.Where(x => x.ContentTypeID == contentTypeID).Select(x => x.StatusInd).FirstOrDefault();
            bool IsSelected = false;
            foreach (KeyValuePair<string, string> stat in StatusTypes)
            {
                if (stat.Value == selectedStatus.ToString()) { IsSelected = true; }
                items.Add(new SelectListItem { Text = stat.Key, Value = stat.Value, Selected = IsSelected });
            }
            SelectList objinfo = new SelectList(items, "Value", "Text", value);
            return objinfo;
        }

        public string GetCategoryName(long id)
        {
            var tmi = Convert.ToInt64(GalleryListingService.TypeMaster.SchoolCategory);
            return _context.Schools.Where(x => x.TypeMasterID == tmi && x.SchoolID == id).Select(x => x.NameTxt).FirstOrDefault();
        }

    }
}