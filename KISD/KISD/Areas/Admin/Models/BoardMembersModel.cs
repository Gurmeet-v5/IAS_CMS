using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class BoardMembersModel
    {
        public long BoardMemberID { get; set; }
        public string NameTxt { get; set; }
        public string TitleTxt { get; set; }
        public string ImageURLTxt { get; set; }
        public string URLTxt { get; set; }
        public string ContactInfoTxt { get; set; }
        public string TermTxt { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public string DescriptionTxt { get; set; }
        public Nullable<long> DisplayOrderNbr { get; set; }
        public string PageMetaTitleTxt { get; set; }
        public string PageMetaDescriptionTxt { get; set; }

        [Required(ErrorMessage = "This field is required.")]
        public bool StatusInd { get; set; }
        public Nullable<DateTime> BOMCreateDate { get; set; }
        public string strBOMCreateDate { get; set; }
        public DateTime CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }
        public SelectList DisplayOrderNbrSelect { get; set; }
        public string[] SelectedRightSections { get; set; }
        public SelectList RightSections { get; set; }
        public List<RightSection> ParentRightSections { get; set; }
        public Nullable<long> BannerImageID { get; set; }
        public string AltBannerImageTxt { get; set; }
        public string BannerImageAbstractTxt { get; set; }
    }
    public class BoardMembersModelService
    {
        private db_KISDEntities _context;
        public BoardMembersModelService()
        {
            _context = new db_KISDEntities();
        }


        /// <summary>
        /// this will return list of all Videos in Video model
        /// </summary>
        /// <returns></returns>
        public List<BoardMembersModel> GetBoardMembers()
        {
            var list = new List<BoardMembersModel>();
            foreach (var x in GetAllBoardMembers())
            {
                list.Add(new BoardMembersModel
                {
                    BoardMemberID = x.BoardMemberID,
                    TitleTxt = x.TitleTxt,
                    BOMCreateDate = x.BOMCreateDate.HasValue ? x.BOMCreateDate.Value : DateTime.Now,
                    ImageURLTxt = x.ImageURLTxt,
                    CreateDate = x.CreateDate.HasValue ? x.CreateDate.Value : DateTime.Now,
                    StatusInd = x.StatusInd.Value,
                    DisplayOrderNbr = x.DisplayOrderNbr,
                    ContactInfoTxt = x.ContactInfoTxt,
                    CreateByID = x.CreateByID,
                    DescriptionTxt = x.DescriptionTxt,
                    IsDeletedInd = x.IsDeletedInd,
                    LastModifyByID = x.LastModifyByID,
                    LastModifyDate = x.LastModifyDate.HasValue ? x.LastModifyDate.Value : DateTime.Now,
                    NameTxt = x.NameTxt,
                    PageMetaDescriptionTxt = x.PageMetaDescriptionTxt,
                    PageMetaTitleTxt = x.PageMetaTitleTxt,
                    RightSectionAbstractTxt = x.RightSectionAbstractTxt,
                    RightSectionTitleTxt = x.RightSectionTitleTxt,
                    TermTxt = x.TermTxt,
                    URLTxt = x.URLTxt,
                    strBOMCreateDate = x.BOMCreateDate.HasValue ? x.BOMCreateDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString(),
                    DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0"))
                });
            }
            return list;
        }

        public IQueryable<BoardOfMember> GetAllBoardMembers()
        {
            return _context.BoardOfMembers.Where(x => x.IsDeletedInd == false);
        }

        public SelectList GetDisplayOrder(string value)
        {
            List<DisplayOrder> status = new List<DisplayOrder>();
            var varCount = _context.BoardOfMembers.Where(x => x.DisplayOrderNbr != 0 && x.IsDeletedInd == false).Count();
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
                    var List = _context.BoardOfMembers.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.IsDeletedInd == false).ToList();
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
                    var List = _context.BoardOfMembers.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.IsDeletedInd == false).ToList();
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


        public bool ChangeDeletedDisplayOrder(long sourceorder, long BOMID)
        {
            try
            {
                var _objBoardOfMembers = _context.BoardOfMembers.Where(x => x.BoardMemberID == BOMID).FirstOrDefault();
                var _obj_objList = _context.BoardOfMembers.Where(x => x.DisplayOrderNbr > sourceorder && x.IsDeletedInd == false).ToList();
                foreach (var item in _obj_objList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objBoardOfMembers.DisplayOrderNbr = null;
                _objBoardOfMembers.IsDeletedInd = true;
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