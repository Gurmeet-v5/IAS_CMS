namespace KISD.Areas.BlogAdmin.Models
{
    public class ReceivingEmailModel
    {
        public ReceivingEmailModel()
        {
            this.ReceivingEmailID = 0;
            this.ReceivingEmailTxt = "";
        }
        public ReceivingEmailModel(int ReceivingEmailID, string ReceivingEmailTxt)
        {
            this.ReceivingEmailID = ReceivingEmailID;
            this.ReceivingEmailTxt = ReceivingEmailTxt;
        }
        public int ReceivingEmailID { get; set; }
        public string ReceivingEmailTxt { get; set; }

    }
}