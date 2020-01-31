namespace KISD.Areas.BlogAdmin.Models
{
    public class EmailsModel
    {
        public EmailsModel()
        {
            this.EmailID = 0;
            this.EmailTxt = "";
            this.EmailTypeID = 0;
        }

        public EmailsModel(int EmailID,string EmailTxt,int EmailTypeID)
        {
            this.EmailID = EmailID;
            this.EmailTxt = EmailTxt;
            this.EmailTypeID = EmailTypeID;
        }
        public int EmailID { get; set; }
        public string EmailTxt { get; set; }
        public int EmailTypeID { get; set; }
    }
    public class EmailTypeModel
    {
        public int EmailTypeID { get; set; }
        public int EmailID { get; set; }

        public string EmailTypeNameTxt { get; set; }

        public EmailTypeModel()
        {
            this.EmailTypeID = 0;
            this.EmailTypeNameTxt = "";
        }

        public EmailTypeModel(int EmailTypeID, string EmailTypeNameTxt)
        {
            this.EmailTypeID = EmailTypeID;
            this.EmailTypeNameTxt = EmailTypeNameTxt;
        }
    }

   
}
