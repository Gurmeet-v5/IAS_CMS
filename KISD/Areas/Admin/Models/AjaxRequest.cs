namespace KISD.Areas.Admin.Models
{
    public class AjaxRequest
    {
        public string ajaxcall { get; set; }
        public string hfid { get; set; }
        public string hfvalue { get; set; }
        public string qs_shownonhomevalue { get; set; }
        public string qs_checkboxselected { get; set; }
        public string qs_value { get; set; }
        public string qs_Type { get; set; }
        public string qs_FilterFromDate { get; set; }
        public string qs_FilterToDate { get; set; }
        public string qs_FilterUserName { get; set; }
        public string qs_Date { get; set; }
        public string qs_CreateDate { get; set; }
        public string qs_FilterUserType { get; set; }
        public string qs_FilterModuleType { get; set; }
    }
}