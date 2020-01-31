namespace KISD.Areas.BlogAdmin.Models
{
    public class CategoriesModel
    {
        public CategoriesModel()
        {
            this.CategoryID = 0;
            this.CategoryNameTxt = "";
        }

        public CategoriesModel(int CategoryID,string NameTxt)
        {
            this.CategoryID = CategoryID;
            this.CategoryNameTxt = NameTxt;
        }

        public int CategoryID { get; set; }
        public string CategoryNameTxt { get; set; }
    }
}