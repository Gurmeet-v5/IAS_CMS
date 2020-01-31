using KISD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.Admin.Models
{
    public class ListingParameterModel
    {
        public int ListingParameterID { get; set; }
        public string ListingParameterTxt { get; set; }
        public string DescriptionTxt { get; set; }
    }

    public class ListingParameterService
    {
        private db_KISDEntities _context;
        public ListingParameterService()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// Get Queryable model of Listing Parameters for defined type
        /// </summary>
        /// <returns></returns>
        public IQueryable<ListingParameterModel> GetListingParameter()
        {
            var query = from a in GetAllListingParameter()
                        select new ListingParameterModel
                        {
                            ListingParameterID = a.ListingParameterID,
                            ListingParameterTxt = a.ListingParameterTxt,
                            DescriptionTxt = a.DescriptionTxt
                        };
            return query;
        }
        /// <summary>
        /// Get all listing parameters of defined type
        /// </summary>
        /// <returns></returns>
        public IQueryable<ListingParameter> GetAllListingParameter()
        {
            return _context.ListingParameters;
        }

    }
}