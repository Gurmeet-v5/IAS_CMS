namespace KISD.Areas.BlogAdmin.Models
{
    public class AjaxRequest
    {
        public string ajaxcall { get; set; }
        public string hfid { get; set; }
        public string hfvalue { get; set; }

        //-------News Section parameters
        public string qs_shownonhomevalue { get; set; }
        public string qs_checkboxselected { get; set; }
        public string qs_value { get; set; }
        public string qs_Type { get; set; }
    }
}