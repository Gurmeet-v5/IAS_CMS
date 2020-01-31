using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using KISD.Areas.BlogAdmin.Models;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class RolesContexts
    {
        private List<RoleModel> allRoles;
        private XDocument RolesData;
        public RolesContexts()
        {
            try
            {
                allRoles = new List<RoleModel>();
                RolesData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/roles.xml"));
                var Roles = from t in RolesData.Descendants("Role")
                            select new RoleModel(
                                (short)t.Element("RoleID"),
                                t.Element("RoleNameTxt").Value,
                            t.Element("RoleDescriptionTxt").Value,
                            t.Element("LoweredRoleNameTxt").Value);

                allRoles.AddRange(Roles.ToList<RoleModel>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// Gets information from the data source for a role. 
        /// </summary>
        /// <returns>A Role object populated with all role's information from the data source.</returns>
        public IEnumerable<RoleModel> GetRoles()
        {
            return allRoles;
        }
        /// <summary>
        /// Gets information from the data source for a role. 
        /// </summary>
        /// <param name="roleName">The name of the role to get information for.</param>
        /// <returns>A Role object populated with the specified role's information from the data source.</returns>
        public RoleModel GetRole(string roleName)
        {
            return allRoles.Find(item => item.RoleName == roleName);
        }

        


        public void AddRole(RoleModel _RolesModel)
        {
            _RolesModel.RoleId = (short)((int)(from S in RolesData.Descendants("Role") orderby (short)S.Element("RoleID") descending select (short)S.Element("RoleID")).FirstOrDefault()+1);
            RolesData.Root.Add(new XElement("Role", new XElement("RoleID", _RolesModel.RoleId),
                               new XElement("RoleNameTxt", _RolesModel.RoleName),
                               new XElement("RoleDescriptionTxt", _RolesModel.RoleDescription),
                               new XElement("LoweredRoleNameTxt", _RolesModel.LoweredRoleNameTxt)));

            RolesData.Save(HttpContext.Current.Server.MapPath("~/App_Data/roles.xml"));
        }

        public void EditRole(RoleModel _RoleModel)
        {
            try
            {
                XElement node = RolesData.Root.Elements("Role").Where(i => (int)i.Element("RoleID") == _RoleModel.RoleId).FirstOrDefault();

                node.SetElementValue("RoleNameTxt", _RoleModel.RoleName);
                node.SetElementValue("RoleDescriptionTxt", _RoleModel.RoleDescription);
                node.SetElementValue("LoweredRoleNameTxt", _RoleModel.LoweredRoleNameTxt);
                RolesData.Save(HttpContext.Current.Server.MapPath("~/App_Data/roles.xml"));
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }

        public void DeleteRoles(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    RolesData.Root.Elements("Role").Where(i => (int)i.Element("RoleID") == id).Remove();

                    RolesData.Save(HttpContext.Current.Server.MapPath("~/App_Data/roles.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }
    }
}