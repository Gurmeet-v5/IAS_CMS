using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.BlogAdmin.Models
{
    public class CropImageModel
    {
        /// <summary>
        /// Image to be cropped save on this path.
        /// </summary>
        public string Imagepath { get; set; }
        /// <summary>
        /// Ideal Height of image.
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// Ideal width of image.
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// Name of image
        /// </summary>
        public string ImageName { get; set; }
        /// <summary>
        /// X axis for image.
        /// </summary>
        public int Xaxis { get; set; }
        /// <summary>
        /// Y axis for image.
        /// </summary>
        public int Yaxis { get; set; }
        /// <summary>
        /// Return file is cropped sucssesfully.
        /// </summary>
        public bool IsFileCroped { get; set; }
        /// <summary>
        /// User cropped the file or Cancel.
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Css Name of file uploader.
        /// </summary>
        public string FileUploaderCss { get; set; }
    }

}