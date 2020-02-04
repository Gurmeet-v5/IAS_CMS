using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class Common
    {
        public enum UserType : int
        {
            SuperAdmin = 1,
            Admin = 2,
            DepartmentUser = 3,
            User = 4
        }

        public enum ListingParameters : int
        {
            FAQ = 1,
            FAQByCategory = 2,
            PhotoGallery = 3,
            PhotoGalleryByCategory = 4,
            PodCast = 5,
            Video = 6,
        }

        public enum ModuleType : int
        {
            Images = 1,
            Email = 2,
            Masters = 3,
            Users = 4,
            Home = 5,
            AboutUs = 6,
            School = 7,
            NewToKISD = 8,
            Departments = 9,
            ParentStudents = 10,
            SchoolBoard = 11,
            Employment = 12,
            Reports = 13,
            News = 14,
            Events = 15,
            Search = 16,
            GoogleAnalytic = 17,
            FlyPages = 18,
            RightSection = 19,
            ListingParameters = 20,
            ContactUs = 21,
            Syllabus = 22,
            DailyNews = 23,
            Video = 24,
            Downloads = 25
        }

        public const int _pageSize = 10;
        public const int _currentPage = 1;
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
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/PodCast")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/PodCast"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/Video")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/Video"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/DocumentViewer")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/DocumentViewer"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/ImageListing")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/ImageListing"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/PhotoGallery")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/PhotoGallery"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/PhotoGallery/Images")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/PhotoGallery/Images"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/PhotoGallery/Thumbnails")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/PhotoGallery/Thumbnails"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/PhotoGallery/cropped")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/PhotoGallery/cropped"));
            }

        }


        /// <summary>
        /// create images and Thumbnails in webdata folder
        /// </summary>
        public static void CreateDocFolder()
        {
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/Resume")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/Resume"));
            }
            if (!Directory.Exists(HttpContext.Current.Server.MapPath("~/WebData/Attachments")))
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath("~/WebData/Attachments"));
            }
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
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    File.Delete(path.Trim());
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// This method is used to bind the status data
        /// </summary>
        /// <returns></returns>
        public static List<SelectListItem> GetStatusData(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();
            data.Text = "Active";
            data.Value = "1";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "InActive";
            data.Value = "0";
            items.Add(data);
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
            data.Value = "True";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "InActive";
            data.Value = "False";
            items.Add(data);
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
        public static List<SelectListItem> GetShowonHomeData(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();
            data.Text = "Yes";
            data.Value = "1";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "No";
            data.Value = "0";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }

        public static List<SelectListItem> GetShowonHomeDataBoolean(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();
            data.Text = "Yes";
            data.Value = "True";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "No";
            data.Value = "False";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }

        public static List<SelectListItem> GetDataBoolean(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();
            data.Text = "Electric";
            data.Value = "Electric";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "Gas";
            data.Value = "Gas";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "Electric/Gas";
            data.Value = "Electric/Gas";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
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

        public static void createthumbnail(string imagepath, int imageWidth, string savetopath)
        {
            try
            {
                int newheight = 90;
                var myImage = System.Drawing.Image.FromFile(imagepath);
                if (myImage.Width > imageWidth)
                {
                    double ratio = Convert.ToDouble(imageWidth) / Convert.ToDouble(myImage.Width);
                    double newheight4 = ratio * Convert.ToDouble(myImage.Height);
                    newheight = Convert.ToInt32(newheight4);
                }
                else
                {
                    imageWidth = myImage.Width;
                    newheight = myImage.Height;
                }
                Bitmap bmp = new Bitmap(imageWidth, newheight);
                Graphics gr = Graphics.FromImage(bmp);
                gr.SmoothingMode = SmoothingMode.HighQuality;
                gr.CompositingQuality = CompositingQuality.HighQuality;
                gr.InterpolationMode = InterpolationMode.High;
                Rectangle rectDestination = new Rectangle(0, 0, imageWidth, newheight);
                gr.DrawImage(myImage, rectDestination, 0, 0, myImage.Width, myImage.Height, GraphicsUnit.Pixel);
                bmp.Save(savetopath);
                bmp.Dispose();
                myImage.Dispose();
            }
            catch
            { }

        }

        internal static dynamic GetWDCN(string value)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();
            data.Text = "Yes";
            data.Value = "Yes";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "No";
            data.Value = "No";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "WD";
            data.Value = "WD";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "WCO";
            data.Value = "WCO";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }

        /// <summary>
        /// Get all Emails of defined type
        /// </summary>
        /// <param name="EmailType"></param>
        /// <returns></returns>
        public static bool IsURLExists(string URL, Int32 ID)
        {
            var count = 0;
            var _objContext = new db_KISDEntities();
            //count = _objContext.News.Where(x => x.PageURLTxt == URL && x.NewsID != ID).Count();
            count += _objContext.Contents.Where(x => x.PageURLTxt == URL).Count();
            //count += _objContext.Categories.Where(x => x.PageUrlTxt == URL).Count();
            if (URL.Trim().ToLower() == "error404")
            {
                count = count + 1;
            }
            return count > 0;
        }

        /// <summary>
        /// Returns Valid URL
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReturnValidPath(string path)
        {
            if (path.ToLower().Contains("http://KISD.centextechdemo3.com/KISD/"))
            {
                path = path.Replace("http://KISD.centextechdemo3.com/KISD/", "http://KISD.centextechdemo3.com/");
            }
            if (path.ToLower().Contains("http://KISD.centextechdemo3.com/KISD"))
            {
                path = path.Replace("http://KISD.centextechdemo3.com/KISD", "http://KISD.centextechdemo3.com");
            }
            return path;
        }

        public static DataTable LINQResultToDataTable<T>(IEnumerable<T> Linqlist)
        {
            DataTable dt = new DataTable();
            PropertyInfo[] columns = null;
            if (Linqlist == null) return dt;
            foreach (T Record in Linqlist)
            {
                if (columns == null)
                {
                    columns = ((Type)Record.GetType()).GetProperties();
                    foreach (PropertyInfo GetProperty in columns)
                    {
                        Type colType = GetProperty.PropertyType;
                        if (colType.Name == "ICollection`1")
                        { }
                        else
                        {
                            if ((colType.IsGenericType) && (colType.GetGenericTypeDefinition()
                            == typeof(Nullable<>)))
                            {
                                colType = colType.GetGenericArguments()[0];
                            }

                            dt.Columns.Add(new DataColumn(GetProperty.Name, colType));
                        }
                    }
                }

                DataRow dr = dt.NewRow();

                foreach (PropertyInfo pinfo in columns)
                {
                    Type colType = pinfo.PropertyType;
                    if (colType.Name == "ICollection`1")
                    {
                    }
                    else
                    {
                        dr[pinfo.Name] = pinfo.GetValue(Record, null) == null ? DBNull.Value : pinfo.GetValue
                    (Record, null);
                    }
                }

                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static bool GetAccessibleTabAccess(int TabID, int userId)
        {
            db_KISDEntities objDB = new db_KISDEntities();
            var AccessibleTabs = objDB.UserPermissions.Where(x => x.UserID == userId && (x.PageID == TabID || x.PageID == 1)).Select(x => x.UserPermissionID).Count();
            bool IsAccessible = AccessibleTabs > 0 ? true : false;
            return IsAccessible;
        }

        public static List<ImageModel> GetInnerImages()
        {
            var objContext = new db_KISDEntities();
            var InnerImagesTitle = (from db in objContext.Images
                                    where db.ImageTypeID == 2 && db.StatusInd == true && db.IsDeletedInd == false
                                    select new ImageModel { TitleTxt = db.TitleTxt, ImageID = db.ImageID }).OrderBy(x => x.TitleTxt).ToList();
            return InnerImagesTitle;
        }
    }

    public class Utility
    {
        public byte[] GetCaptchaImage(string checkCode)
        {
            var image = new Bitmap(Convert.ToInt32(Math.Ceiling((decimal)(checkCode.Length * 25))), 35);
            var g = Graphics.FromImage(image);
            try
            {
                var random = new Random();
                g.Clear(Color.DimGray);
                var font = new Font("Comic Sans MS", 14, FontStyle.Bold);
                string str = "";
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(20, 10, image.Width, image.Height), Color.Blue, Color.DarkRed, 3.2f, true);
                for (int i = 0; i < checkCode.Length; i++)
                {
                    str = str + checkCode.Substring(i, 1);
                }
                g.DrawString(str, font, new SolidBrush(Color.Black), 0, 0);
                g.Flush();
                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
                return ms.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        public byte[] VerificationTextGenerator()
        {
            string captuchaCharacters = "123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var objRandomNumber = new Random(DateTime.Now.Millisecond);
            var legalChars = captuchaCharacters;
            var sb = new StringBuilder();

            for (int i = 0; i < 6; i++)
            {
                sb.Append(legalChars.Substring(objRandomNumber.Next(0, legalChars.Length - 1), 1));
                System.Threading.Thread.Sleep(30);
            }
            HttpContext.Current.Session["Captcha"] = sb.ToString();
            return GetCaptchaImage(sb.ToString());
        }

        /// <summary>
        /// get last identity of inserted Album
        /// </summary>
        /// <returns></returns>
        public static long AlbumLastIdentiy()
        {
            db_KISDEntities objPhotoPortal = new db_KISDEntities();
            return Convert.ToInt64(objPhotoPortal.Database.SqlQuery<decimal>("Select IDENT_CURRENT ('Category')", new object[0]).FirstOrDefault());
        }
    }

    public static class EncryptDecrypt
    {
        private const string initVector = "tu89geji340t89u2";

        private const int keysize = 256;
        public static string Encrypt(string Text)
        {
            string Key = "ki2k3aik3h";
            byte[] initVectorBytes = Encoding.UTF8.GetBytes(initVector);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(Text);
            PasswordDeriveBytes password = new PasswordDeriveBytes(Key, null);
            byte[] keyBytes = password.GetBytes(keysize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes);
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] Encrypted = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(Encrypted);
        }

        public static string Decrypt(string EncryptedText)
        {
            try
            {
                string Key = "ki2k3aik3h";
                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] DeEncryptedText = Convert.FromBase64String(EncryptedText.Replace(" ", "+"));
                PasswordDeriveBytes password = new PasswordDeriveBytes(Key, null);
                byte[] keyBytes = password.GetBytes(keysize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes);
                MemoryStream memoryStream = new MemoryStream(DeEncryptedText);
                CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                byte[] plainTextBytes = new byte[DeEncryptedText.Length];
                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
            catch (Exception ex)
            {
                string s = "";
                return s;
            }
        }
    }
}

