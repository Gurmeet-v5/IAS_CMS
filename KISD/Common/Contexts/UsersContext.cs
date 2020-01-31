using System.Data.Entity;
using System.Linq;

namespace KISD.Common.Context
{
    public class UsersContext : DbContext
    {
        db_KISDEntities objDBContext = new db_KISDEntities();

        /// <summary>
        /// Adds a new user to the data source.
        /// </summary>
        /// <param name="user">The User object which contains the user information.</param>
        public void AddUser(User user)
        {
            objDBContext.Users.Add(user);
            objDBContext.SaveChanges();
        }

        /// <summary>
        /// Gets information from the data source for a user. 
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <returns>A User object populated with the specified user's information from the data source.</returns>
        public User GetUser(string userName)
        {
            var user = objDBContext.Users.SingleOrDefault(u => u.UserNameTxt == userName);
            return user;
        }

        /// <summary>
        /// Gets information from the data source for a user. 
        /// </summary>
        /// <param name="userName">The name of the user to get information for.</param>
        /// <param name="password">The password of the user to get information for.</param>
        /// <returns>A User object populated with the specified user's information from the data source.</returns>
        public User GetUser(string userName, string password)
        {
            User users = objDBContext.Users.SingleOrDefault(u => u.UserNameTxt == userName && u.PasswordTxt == password && u.StatusInd == true && (u.IsDeletedInd == false || u.IsDeletedInd == null));
            return users;
        }

        /// <summary>
        /// Adds a new user role to the data source.
        /// </summary>
        /// <param name="userRole">The UserRoleModel object which contains the user role information.</param>
        public void AddUserRole(Areas.Admin.Models.UserRoleModel userRole)
        {
            var roleEntry = objDBContext.UserRoles.SingleOrDefault(r => r.User.UserID == userRole.UserID);
            if (roleEntry != null)
            {
                objDBContext.UserRoles.Remove(roleEntry);
                SaveChanges();
            }
            UserRole ur = new UserRole()
            {
                Role = objDBContext.Roles.Find(userRole.RoleID),
                User = objDBContext.Users.Find(userRole.UserID)
            };
            objDBContext.UserRoles.Add(ur);
            objDBContext.SaveChanges();
        }

    }
}