namespace KISD.Areas.BlogAdmin.Models
{
    public class SocialMediasModel
    {
        public SocialMediasModel()
        {
            this.SocialMediaID = 0;
            this.SocialMediaNameTxt = "";
        }
        public SocialMediasModel(int SocialMediaID, string SocialMediaNameTxt)
        {
            this.SocialMediaID = SocialMediaID;
            this.SocialMediaNameTxt = SocialMediaNameTxt;
        }
        public int SocialMediaID { get; set; }
        public string SocialMediaNameTxt { get; set; }
    }
}
