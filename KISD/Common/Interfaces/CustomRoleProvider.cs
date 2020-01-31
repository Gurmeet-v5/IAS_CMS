using System;
using System.Linq;
using System.Web.Security;
using KISD.Common.Context;

namespace KISD.Common.Interfaces
{
    public class CustomRoleProvider : RoleProvider
    {
        db_KISDEntities obj = new db_KISDEntities();
        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role. 
        /// </summary>
        /// <param name="username">The name of the user to search for.</param>
        /// <param name="roleName">The name of the role to search in.</param>
        /// <returns>true if the specified user is in the specified role; otherwise, false.</returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            return IsUserInRoleExist(username, roleName);
        }

        public bool IsUserInRoleExist(string username, string roleName)
        {
            using (var usersContext = new UsersContext())
            {
                var user = obj.Users.SingleOrDefault(u => u.UserNameTxt == username);
                // var user = usersContext.Users.SingleOrDefault(u => u.UserName == username);
                if (user == null)
                    return false;
                return user.UserRoles != null && user.UserRoles.Select(u => u.Role).Any(r => r.RoleNameTxt == roleName);
            }
        }
        /// <summary>
        /// Gets a list of the roles that a specified user is in for this application.
        /// </summary>
        /// <param name="username">The name of the user to search for.</param>
        /// <returns>A string array containing the names of all the roles that the specified user is in for the configured applicationName.</returns>
        public override string[] GetRolesForUser(string username)
        {
            using (var usersContext = new UsersContext())
            {
                var user = obj.Users.SingleOrDefault(u => u.UserNameTxt == username);
                // var user = usersContext.Users.SingleOrDefault(u => u.UserName == username);
                if (user == null)
                    return new string[] { };
                return user.UserRoles == null ? new string[] { } : user.UserRoles.Select(u => u.Role).Select(u => u.RoleNameTxt).ToArray();
            }
        }

        /// <summary>
        /// Adds a new role to the data source for the this application.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a role from the data source for the this application.
        /// </summary>
        /// <param name="roleName">The name of the role to create.</param>
        /// <param name="throwOnPopulatedRole">If true, throw an exception if roleName has one or more members and do not delete roleName.</param>
        /// <returns>true if the role was successfully deleted; otherwise, false.</returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            bool flag = false;
            try
            {
                var thisRole = obj.Roles.Where(r => r.RoleNameTxt.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                obj.Roles.Remove(thisRole);
                obj.SaveChanges();
                flag = true;
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>true if the role name already exists in the data source for the configured applicationName; otherwise, false.</returns>
        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds the specified user names to the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be added to the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            var un = usernames[0];
            var rn = roleNames[0];
            var user = obj.Users.Where(u => u.UserNameTxt.Equals(un, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            var role = obj.Roles.Where(u => u.RoleNameTxt.Equals(rn, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            UserRole ur = new UserRole();
            ur.User = user;
            ur.Role = role;
            obj.UserRoles.Add(ur);
            obj.SaveChanges();
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of the role names to add the specified user names to.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {

            var un = usernames[0];
            var rn = roleNames[0];
            var user = obj.Users.Where(u => u.UserNameTxt.Equals(un, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            if (IsUserInRoleExist(user.UserNameTxt, rn))
            {
                var userrole = (from u in obj.UserRoles
                                join
                                rol in obj.Roles on u.Role.RoleID equals rol.RoleID
                                where u.User.UserNameTxt.Equals(un, StringComparison.CurrentCultureIgnoreCase) && rol.RoleNameTxt.Equals(rn, StringComparison.CurrentCultureIgnoreCase)
                                select u).FirstOrDefault();
                UserRole ur = obj.UserRoles.Find(userrole.UserRoleID);
                obj.UserRoles.Remove(ur);
                obj.SaveChanges();
            }
        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>A string array containing the names of all the users who are members of the specified role for the configured applicationName.</returns>
        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>A string array containing the names of all the roles stored in the data source for the configured applicationName.</returns>
        public override string[] GetAllRoles()
        {
            //using (var usersContext = new UsersContext())
            //{
            //    return usersContext.Roles.Select(r => r.RoleName).ToArray();
            //}
            return obj.Roles.Select(r => r.RoleNameTxt).ToArray();
        }

        /// <summary>
        /// Gets an array of user names in a role where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="roleName">The role to search in.</param>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <returns>A string array containing the names of all the users where the user name matches usernameToMatch and the user is a member of the specified role.</returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// To override the application name.
        /// </summary>
        public override string ApplicationName { get; set; }
    }
}