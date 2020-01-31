using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using KISD.Areas.BlogAdmin.Models;
using System.Xml.Linq;
using System.Web.Mvc;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class UsersContexts
    {
        
        private List<UsersModel> allUsers;
        private XDocument UsersData;
        
        public UsersContexts()
        {
            try
            {
                allUsers = new List<UsersModel>();
                UsersData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/users.xml"));
                var Users = from t in UsersData.Descendants("User")
                               select new UsersModel(
                                   (int)t.Element("UserID"),
                                   string.IsNullOrEmpty(t.Element("FirstName").Value) ? "" : t.Element("FirstName").Value,
                                   string.IsNullOrEmpty(t.Element("LastName").Value) ? "" : t.Element("LastName").Value,
                                   string.IsNullOrEmpty(t.Element("UserNameTxt").Value) ? "" : t.Element("UserNameTxt").Value,
                                   string.IsNullOrEmpty(t.Element("Password").Value) ? "" : t.Element("Password").Value,
                                   string.IsNullOrEmpty(t.Element("EmailTxt").Value) ? "" : t.Element("EmailTxt").Value,            
                               (bool)t.Element("StatusInd"));

                allUsers.AddRange(Users.ToList<UsersModel>());
            }
            catch (Exception ex)
            {
                throw new NotImplementedException();
            }
        }
        public IEnumerable<UsersModel> GetAccountUsers()
        {
            return allUsers;
        }
        /// <summary>
        /// Gets information from the data source for a user. 
        /// </summary>
        /// <returns>A User object populated with all user's information from the data source.</returns>
        public IEnumerable<UsersModel> GetUsers()
        {
            return allUsers;
        }
        public IEnumerable<UsersModel> GetLoginUser(string userName)
        {
            return allUsers.Where(x => x.UserNameTxt == userName);
        }
        /// <summary>
        /// Gets information from the data source for a user by username. 
        /// </summary>
        /// <param name="userName">The name of the user to get information for.</param>
        /// <returns>A User object populated with the specified user's information from the data source.</returns>

        public UsersModel GetUser(string userName)
        {
            return allUsers.Find(item => item.UserNameTxt == userName);
        }


        public IEnumerable<UsersModel> GetUserList(string value)
        {
            var objallUsers = new List<UsersModel>();
            if (!string.IsNullOrEmpty(value))
            {
                objallUsers = allUsers.Where(x => x.StatusInd == true || x.UserID == Convert.ToInt32(value)).ToList();
            }
            else
            {
                objallUsers = allUsers.Where(x => x.StatusInd == true).ToList();
            }
            var objUsersModel = new UsersModel();
            objUsersModel.UserNameTxt = "-- Select Author --";
            objUsersModel.UserID = 0;
            objallUsers.Insert(0, objUsersModel);
            List<SelectListItem> Selectitems = new List<SelectListItem>();
            foreach (var item in objallUsers)
            {
                SelectListItem data = new SelectListItem();
                data.Text = item.UserNameTxt;
                data.Value = Convert.ToString(item.UserID);
                Selectitems.Add(data);
            }
            if (!string.IsNullOrEmpty(value))
            {
                Selectitems.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }

            return objallUsers as IEnumerable<UsersModel>;
        }

        /// <summary>
        /// Gets information from the data source for a user by username and password. 
        /// </summary>
        /// <param name="userName">The name of the user to get information for.</param>
        /// <param name="password">The password of the user to get information for.</param>
        /// <returns>A User object populated with the specified user's information from the data source.</returns>
        public UsersModel GetUser(string userName, string password)
        {
            return allUsers.Find(item => item.UserNameTxt == userName && item.Password==password && item.StatusInd == true);
        }


        public void AddUser(UsersModel _usersModel)
        {
            _usersModel.UserID = (int)(from S in UsersData.Descendants("User") orderby (int)S.Element("UserID") descending select (int)S.Element("UserID")).FirstOrDefault() + 1;
            UsersData.Root.Add(new XElement("User", new XElement("UserID", _usersModel.UserID),
                               new XElement("FirstName", !string.IsNullOrEmpty(_usersModel.FirstName) ? _usersModel.FirstName : ""),
                               new XElement("LastName", !string.IsNullOrEmpty(_usersModel.LastName) ? _usersModel.LastName : ""),
                               new XElement("UserNameTxt", !string.IsNullOrEmpty(_usersModel.UserNameTxt) ? _usersModel.UserNameTxt : ""),
                               new XElement("Password", !string.IsNullOrEmpty(_usersModel.Password) ? _usersModel.Password : ""),
                               new XElement("EmailTxt", !string.IsNullOrEmpty(_usersModel.EmailTxt) ? _usersModel.EmailTxt : ""),
                               new XElement("StatusInd", _usersModel.StatusInd)
                               ));

            UsersData.Save(HttpContext.Current.Server.MapPath("~/App_Data/users.xml"));
        }

        public void EditUser(UsersModel _usersModel)
        {
            try
            {
                XElement node = UsersData.Root.Elements("User").Where(i => (int)i.Element("UserID") == _usersModel.UserID).FirstOrDefault();

                node.SetElementValue("FirstName", !string.IsNullOrEmpty(_usersModel.FirstName) ? _usersModel.FirstName : "");
                node.SetElementValue("LastName", !string.IsNullOrEmpty(_usersModel.LastName) ? _usersModel.LastName : "");
                node.SetElementValue("UserNameTxt", !string.IsNullOrEmpty(_usersModel.UserNameTxt) ? _usersModel.UserNameTxt : "");
                node.SetElementValue("Password", !string.IsNullOrEmpty(_usersModel.Password) ? _usersModel.Password : "");
                node.SetElementValue("StatusInd", _usersModel.StatusInd);
                UsersData.Save(HttpContext.Current.Server.MapPath("~/App_Data/users.xml"));
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }

        public bool ChangePassword(string oldPasswd, string newPasswd, string username)
        {
            var status = false;
            try
            {
                var _UsersContexts = new Areas.BlogAdmin.Contexts.UsersContexts();
                var oldPassword = _UsersContexts.GetUser(username).Password;
                if (oldPassword != oldPasswd)
                {
                    status = false;
                }
                else
                {
                    XElement node = UsersData.Root.Elements("User").Where(i => (string)i.Element("UserNameTxt") == username).FirstOrDefault();

                    node.SetElementValue("Password", !string.IsNullOrEmpty(newPasswd) ? newPasswd : "");

                    UsersData.Save(HttpContext.Current.Server.MapPath("~/App_Data/users.xml"));
                    status = true;
                }
                
            }
            catch (Exception)
            {
                status = false;
                throw new NotImplementedException();
            }
            return status;
        }

        public void DeleteUsers(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    UsersData.Root.Elements("User").Where(i => (int)i.Element("UserID") == id).Remove();

                    UsersData.Save(HttpContext.Current.Server.MapPath("~/App_Data/users.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }

    }
}