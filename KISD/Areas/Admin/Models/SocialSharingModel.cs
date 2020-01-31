using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public enum SectionName : int
    {
        Home = 1,
        AboutKISD = 2,
        School = 3,
        NewToKISD = 4,
        Departments = 5,
        ParentStudents = 6,
        SchoolBoard = 7,
        Employment = 8
    }

    public class SocialSharingModel
    {
        public SelectList SelectedSection { get; set; }
        public SelectList AllSections { get; set; }
    }
}