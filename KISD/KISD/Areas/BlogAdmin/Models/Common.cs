using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.BlogAdmin.Models
{
    public class Common
    {
        public const Int32 _pageSize = 10;
        public const Int32 _currentPage = 1;

        /// <summary>
        /// create images and Thumbnails in webdata folder
        /// </summary>
        public static void CreateFolder()
        {

            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/Images")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/Images"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/Thumbnails")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/Thumbnails"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/Thumbnails_Small")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/Thumbnails_Small"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/Cropped")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/Cropped"));
            }

        }
        /// <summary>
        /// Gets Font Size List
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<SelectListItem> GetFontList(string value)
        {
            int[] fontSize = new int[21] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };

            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();

            foreach (var item in fontSize)
            {
                data = new SelectListItem();
                data.Text = item.ToString();
                data.Value = item.ToString();
                items.Add(data);

            }
            //data.Text = "Active";
            //data.Value = "true";
            //items.Add(data);
            //data = new SelectListItem();
            //data.Text = "Inactive";
            //data.Value = "false";
            //items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }
        /// <summary>
        /// This method is used to bind the status data
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetStatusListBoolean(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();
            data.Text = "Active";
            data.Value = "true";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "InActive";
            data.Value = "false";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value.ToLower()).FirstOrDefault().Selected = true;
            }
            return items;
        }

        /// <summary>
        /// Delete Image at path
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteImage(string path)
        {
            try
            {
                path = path.Replace("~\\", "");
                if (File.Exists(path.Trim()))
                {

                    File.Delete(path.Trim());
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// This method is used to create the thumbnails of the image according to specified width
        /// </summary>
        /// <param name="imagePath">This parameter is used to get the path of the image</param>
        /// <param name="imageWidth">This parameter is used to get the widthof the thumbnails  </param>
        /// <returns>this method returns the generated thumbnail image.</returns>
        public static System.Drawing.Image CreateImageThumbnail(string imagePath, int imageWidth)
        {
            var myImage = System.Drawing.Image.FromFile(imagePath);

            if (myImage.Width > imageWidth)
            {
                double ratio = Convert.ToDouble(imageWidth) / Convert.ToDouble(myImage.Width);
                double newheight4 = ratio * Convert.ToDouble(myImage.Height);
                int newheight = Convert.ToInt32(newheight4);

                myImage = myImage.GetThumbnailImage(imageWidth, newheight, null, new System.IntPtr());
            }
            return myImage;
        }

        /// <summary>
        /// Returns Valid URL
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReturnValidPath(string path)
        {
            if (path.ToLower().Contains("http://vic.centextechdemo3.com/vic/"))// replace abc.com/abc with your url
            {
                path = path.Replace("http://vic.centextechdemo3.com/vic/", "http://vic.centextechdemo3.com/");
            }
            if (path.ToLower().Contains("http://vic.centextechdemo3.com/vic"))
            {
                path = path.Replace("http://vic.centextechdemo3.com/vic", "http://vic.centextechdemo3.com");
            }
            return path;

        }
    }
}