using KISD.Areas.BlogAdmin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class UsersRolesContext
    {
        private List<UserRoleModel> allUsersRole;
        private XDocument UserroleData;
        public UsersRolesContext()
        {
            allUsersRole = new List<UserRoleModel>();
            UserroleData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/usersroles.xml"));
            var Usersrole = from t in UserroleData.Descendants("UserRole")
                            select new UserRoleModel(
                                (short)t.Element("RoleID"),
                                (int)t.Element("UserRoleID"),
                                (int)t.Element("UserID"));
            allUsersRole.AddRange(Usersrole.ToList<UserRoleModel>());
        }
        public IEnumerable<UserRoleModel> GetUsersRoles()
        {
            return allUsersRole;
        }
        /// <summary>
        /// Gets information from the data source for a user. 
        /// </summary>
        /// <returns>A User object populated with all user's information from the data source.</returns>
        public IEnumerable<UserRoleModel> GetUsersRoles_()
        {
            var _objUsersContexts = new Areas.BlogAdmin.Contexts.UsersContexts();
            var _objContext = new Areas.BlogAdmin.Contexts.UsersRolesContext();
            var user = _objUsersContexts.GetUsers().Where(x => x.StatusInd);
            var userlist = new List<UserRoleModel>();
            var allUserRoleList = allUsersRole;
            foreach (var item in user)
            {
                foreach (var inneritem in allUserRoleList)
                {
                    if (item.UserID == inneritem.UserID)
                    {
                        inneritem.UserName = item.UserNameTxt;
                        userlist.Add(inneritem);
                    }
                }
            }
            return userlist;
        }
        public void AssignRoleToUser(UserRoleModel _userRoleModel)
        {
            _userRoleModel.UserRoleID = (int)(from S in UserroleData.Descendants("UserRole") orderby (int)S.Element("UserRoleID") descending select (int)S.Element("UserRoleID")).FirstOrDefault() + 1;
            UserroleData.Root.Add(new XElement("UserRole", new XElement("UserRoleID", _userRoleModel.UserRoleID),
                               new XElement("UserID", _userRoleModel.UserID),
                               new XElement("RoleID", _userRoleModel.RoleID)));
            UserroleData.Save(HttpContext.Current.Server.MapPath("~/App_Data/usersroles.xml"));
        }

        public void EditUserRole(UserRoleModel _userRoleModel)
        {
            try
            {
                XElement node = UserroleData.Root.Elements("UserRole").Where(i => (int)i.Element("UserRoleID") == _userRoleModel.UserRoleID).FirstOrDefault();
                node.SetElementValue("UserID", _userRoleModel.UserID);
                node.SetElementValue("RoleID", _userRoleModel.RoleID);
                UserroleData.Save(HttpContext.Current.Server.MapPath("~/App_Data/usersroles.xml"));
            }
            catch (Exception)
            {
                throw new NotImplementedException();
            }
        }
        public void DeleteUsersRole(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    UserroleData.Root.Elements("UserRole").Where(i => (int)i.Element("UserRoleID") == id).Remove();
                    UserroleData.Save(HttpContext.Current.Server.MapPath("~/App_Data/usersroles.xml"));
                }
                catch (Exception)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}