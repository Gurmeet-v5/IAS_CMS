using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace KISD.Areas.Admin.Models
{
    public class ImageModel
    {
        public long ImageID { get; set; }
        public long ImageTypeID { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string ImgPathTxt { get; set; }
        public string TitleTxt { get; set; }
        public string AltImageTxt { get; set; }
        public string URLTxt { get; set; }
        public Nullable<bool> TargetWindowInd { get; set; }
        public Nullable<System.DateTime> DisplayStartDate { get; set; }
        public Nullable<System.DateTime> DisplayEndDate { get; set; }
        public string AbstractTxt { get; set; }

        public SelectList DisplayOrderNbrSelect { get; set; }//Display Order
        public Nullable<long> DisplayOrderNbr { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public bool StatusInd { get; set; }
        public Nullable<System.DateTime> ImageCreateDate { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:d}")]
        [RegularExpression(@"\d{1,2}/\d{1,2}/\d{4,4}", ErrorMessage = "Invalid date.")]
        public System.DateTime CreateDate { get; set; }
        public Nullable<long> CreateByID { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public Nullable<long> LastModifyByID { get; set; }
        public Nullable<bool> IsDeletedInd { get; set; }

    }
    public class ImageService
    {
        private db_KISDEntities _context;
        public ImageService()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// IQueryable<ImageModel> Method for Getting the Image Model by passing the ImageTypeID
        /// And Query the From database by calling the GetImage() Method.
        /// </summary>
        /// <param name="ImageTypeID">ImageTypeID</param>
        /// <returns>ImageModel</returns>
        public IQueryable<ImageModel> GetImagesView(long ImageTypeID)
        {
            var query = from x in GetImage(ImageTypeID)
                        select new ImageModel
                        {
                            ImageID = x.ImageID,
                            TitleTxt = x.TitleTxt,
                            AbstractTxt = x.AbstractTxt,
                            AltImageTxt = x.AltImageTxt,
                            ImgPathTxt = x.ImgPathTxt,
                            CreateDate = x.CreateDate,
                            ImageTypeID = x.ImageTypeID,
                            StatusInd = x.StatusInd,
                            URLTxt = x.URLTxt,
                            DisplayStartDate = x.DisplayStartDate,
                            DisplayEndDate = x.DisplayEndDate,
                            DisplayOrderNbr = x.DisplayOrderNbr,
                            ImageCreateDate = x.ImageCreateDate,
                            DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0"), x.ImageTypeID)
                        };
            return query.AsQueryable();
        }


        /// <summary>
        /// this will return list of all Videos in Video model
        /// </summary>
        /// <returns></returns>
        public List<ImageModel> GetImages(long ImageTypeID)
        {
            var list = new List<ImageModel>();
            foreach (var x in GetImage(ImageTypeID))
            {
                list.Add(new ImageModel
                {
                    ImageID = x.ImageID,
                    TitleTxt = x.TitleTxt,
                    AbstractTxt = x.AbstractTxt,
                    AltImageTxt = x.AltImageTxt,
                    ImgPathTxt = x.ImgPathTxt,
                    CreateDate = x.CreateDate,
                    ImageTypeID = x.ImageTypeID,
                    StatusInd = x.StatusInd,
                    URLTxt = x.URLTxt,
                    DisplayStartDate = x.DisplayStartDate,
                    DisplayEndDate = x.DisplayEndDate,
                    DisplayOrderNbr = x.DisplayOrderNbr,
                    ImageCreateDate = x.ImageCreateDate,
                    DisplayOrderNbrSelect = GetDisplayOrder((x.DisplayOrderNbr != null ? x.DisplayOrderNbr.Value.ToString() : "0"), x.ImageTypeID)

                });
            }
            return list;
        }

        /// <summary>
        /// IQueryable<ParrieHaynesRanch.Image> get the Image Data by passing the ImageTypeID.
        /// </summary>
        /// <param name="ImageTypeID">ImageTypeID</param>
        /// <returns>ImageObject</returns>
        public IQueryable<Image> GetImage(long? ImageTypeID)
        {
            return _context.Images.Where(x => (x.ImageTypeID == ImageTypeID && x.IsDeletedInd == false));
        }

        /// <summary>
        /// Get all Image of defined type
        /// </summary>
        /// <param name="ImageType"></param>
        /// <returns></returns>
        public string GetImageType(long ImageType)
        {
            return _context.ImageTypes.Where(x => x.ImageTypeID == ImageType).Select(x => x.ImageTypeNameTxt).FirstOrDefault();
        }

        /// <summary>
        ///  All Images with static defined types.        
        /// </summary>        
        /// <returns></returns>
        public enum ImageType : long
        {
            BannerImage = 1,
            InnerImage = 2,
            ImportantInfo = 3,
            Icon = 4
        }

        public SelectList GetDisplayOrder(string value, long ImageTypeID)
        {
            List<DisplayOrder> status = new List<DisplayOrder>();
            var varCount = _context.Images.Where(x => x.DisplayOrderNbr != 0 && x.ImageTypeID == ImageTypeID && x.IsDeletedInd == false).Count();
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
        public static bool ChangeImageDisplayOrder(long sourceorder, long targetorder, long ContentID, int ImageTypeID)
        {
            var _context = new db_KISDEntities();
            try
            {
                if (sourceorder < targetorder)
                {
                    var ImgList = _context.Images.Where(x => x.DisplayOrderNbr >= sourceorder && x.DisplayOrderNbr <= targetorder && x.ImageTypeID == ImageTypeID && x.IsDeletedInd == false).ToList();
                    foreach (var item in ImgList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr -= 1;
                    }
                }
                else
                {
                    var ImgList = _context.Images.Where(x => x.DisplayOrderNbr >= targetorder && x.DisplayOrderNbr <= sourceorder && x.ImageTypeID == ImageTypeID && x.IsDeletedInd == false).ToList();
                    foreach (var item in ImgList)
                    {
                        if (item.DisplayOrderNbr == sourceorder)
                            item.DisplayOrderNbr = targetorder;
                        else
                            item.DisplayOrderNbr += 1;
                    }
                }
                _context.SaveChanges();

                #region System Change Log
                //SystemChangeLog objSCL = new SystemChangeLog();
                //long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                //User objuser = _context.Users.Where(x => x.UserID == userid).FirstOrDefault();
                //objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                //objSCL.UsernameTxt = objuser.UserNameTxt;
                //objSCL.UserRoleID = (short)_context.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                //objSCL.ModuleTxt = "Image";
                //objSCL.LogTypeTxt = images.ImageID > 0 ? "Update" : "Add";
                //objSCL.NotesTxt = "Right Section Details" + (images.ImageID > 0 ? " updated for " : "  added for ") + images.TitleTxt;
                //objSCL.LogDateTime = DateTime.Now;
                //objContext.SystemChangeLogs.Add(objSCL);
                //objContext.SaveChanges();

                //objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                //var newResult = (from x in objContext.Images
                //                 where x.ImageID == images.ImageID
                //                 select x);
                //DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                //foreach (DataColumn col in dtNew.Columns)
                //{
                //    // if(objSCL)
                //    if (dtOld.Rows[0][col.ColumnName].ToString() != dtNew.Rows[0][col.ColumnName].ToString())
                //    {
                //        SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                //        objSCLD.ChangeLogID = objSCL.ChangeLogID;
                //        objSCLD.FieldNameTxt = col.ColumnName.ToString();
                //        objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                //        objSCLD.NewValueTxt = dtNew.Rows[0][col.ColumnName].ToString();
                //        objContext.SystemChangeLogDetails.Add(objSCLD);
                //        objContext.SaveChanges();
                //    }
                //}
                #endregion

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
        /// <param name="ImageID"></param>
        public bool ChangeDeletedDisplayOrder(long sourceorder, long ImageID, long ImageTypeID)
        {
            try
            {
                var _objImage = _context.Images.Where(x => x.ImageID == ImageID).FirstOrDefault();
                var _obj_objMenuList = _context.Images.Where(x => x.DisplayOrderNbr > sourceorder && x.ImageTypeID == ImageTypeID && x.IsDeletedInd == false).ToList();
                foreach (var item in _obj_objMenuList)
                {
                    item.DisplayOrderNbr -= 1;
                }
                _objImage.IsDeletedInd = true;
                _objImage.DisplayOrderNbr = null;
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    //Display Order 
    public class DisplayOrder
    {
        public int DisplayID { get; set; }
        public string DisplayOrderNbr { get; set; }
    }
}