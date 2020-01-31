using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using KISD.Common.Context;
using System.Configuration;
using System.Web;

namespace KISD.Common.Infrastructure
{
    /// <summary>
    /// This class override the functionality of the MembershipProvider.
    /// To Override the MembershipProvider, we have to add the provider for this class in web.config file also. 
    /// If we don't do this then this class cannot override the MembershipProvider.
    /// </summary>
    public class CustomMembershipProvider : MembershipProvider
    {
        /// <summary>
        /// To override the application name.
        /// </summary>
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// To override the change password functionality of the MembershipProvider and implement of its own.
        /// From this user can change the password by providing the new and old password along with the username.
        /// </summary>
        /// <param name="username">User Name.</param>
        /// <param name="oldPassword">The current password for the membership user.</param>
        /// <param name="newPassword">The new password value for the membership user.</param>
        /// <returns>true if the update was successful; otherwise, false.</returns>
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            try
            {
                db_KISDEntities objDBEntities = new db_KISDEntities();
                var userp = (from u in objDBEntities.Users where u.UserNameTxt == username && u.PasswordTxt == oldPassword select u).SingleOrDefault();
                if (userp != null)
                {
                    userp.PasswordTxt = newPassword;
                    objDBEntities.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// To override the ChangePasswordQuestionAndAnswer functionality of the MembershipProvider and implement of its own.
        /// Updates the password question and answer for the membership user in the membership data store.
        /// </summary>
        /// <param name="username">The username for the membership user.</param>
        /// <param name="password">The current password for the membership user.</param>
        /// <param name="newPasswordQuestion">The new password question value for the membership user.</param>
        /// <param name="newPasswordAnswer">The new password answer value for the membership user.</param>
        /// <returns>true if the update was successful; otherwise, false.</returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// To override the CreateUser functionality of the MembershipProvider and implement of its own.
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user.</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A MembershipCreateStatus enumeration value indicating whether the user was created successfully.</param>
        /// <returns>A MembershipUser object populated with the information for the newly created user.</returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            if (RequiresUniqueEmail && GetUserNameByEmail(email) != string.Empty)
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            var user = GetUser(username, true);

            if (user == null)
            {
                var userObj = new User { UserNameTxt = username, PasswordTxt = GetMd5Hash(password), EmailTxt = email };

                using (var usersContext = new UsersContext())
                {
                    usersContext.AddUser(userObj);
                }

                status = MembershipCreateStatus.Success;

                return GetUser(username, true);
            }
            status = MembershipCreateStatus.DuplicateUserName;

            return null;
        }


        /// <summary>
        /// To override the DeleteUser functionality of the MembershipProvider and implement of its own.
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>true if the user was successfully deleted; otherwise, false.</returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            db_KISDEntities objDBEntities = new db_KISDEntities();
            var userp = (from u in objDBEntities.Users where u.UserNameTxt == username select u).SingleOrDefault();
            if (deleteAllRelatedData)
            {
                var role = (from u in objDBEntities.UserRoles where u.UserID == userp.UserID select u).FirstOrDefault();
                if (role != null)
                {
                    objDBEntities.UserRoles.Remove(role);
                    objDBEntities.SaveChanges();
                }
            }

            objDBEntities.Users.Remove(userp);
            objDBEntities.SaveChanges();

            return true;
            // throw new NotImplementedException();
        }

        /// <summary>
        /// To override the EnablePasswordReset functionality of the MembershipProvider and implement of its own.
        /// Gets a value indicating whether the current membership provider is configured to allow users to reset their passwords.
        /// </summary>
        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// To override the EnablePasswordRetrieval functionality of the MembershipProvider and implement of its own.
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// To override the MembershipUserCollection functionality of the MembershipProvider and implement of its own.
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>A MembershipUserCollection collection that contains a page of pageSize MembershipUser objects beginning at the page specified by pageIndex.</returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">he user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>A MembershipUserCollection collection that contains a page of pageSize MembershipUser objects beginning at the page specified by pageIndex.</returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. pageIndex is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>A MembershipUserCollection collection that contains a page of pageSize MembershipUser objects beginning at the page specified by pageIndex.</returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>The number of users currently accessing the application.</returns>
        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns></returns>
        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>A MembershipUser object populated with the specified user's information from the data source.</returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            var usersContext = new UsersContext();
            var user = usersContext.GetUser(username);
            if (user != null)
            {
                var memUser = new MembershipUser("CustomMembershipProvider", username, user.UserID, user.EmailTxt,
                                                            string.Empty, string.Empty,
                                                            true, false, DateTime.MinValue,
                                                            DateTime.MinValue,
                                                            DateTime.MinValue,
                                                            DateTime.Now, DateTime.Now);
                return memUser;
            }   //  user.UserEmailAddress
            return null;
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>A MembershipUser object populated with the specified user's information from the data source.</returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            db_KISDEntities objDBEntities = new db_KISDEntities();
            var username = (from u in objDBEntities.Users where u.UserID == (long)providerUserKey select u.UserNameTxt).SingleOrDefault();
            var usersContext = new UsersContext();
            var user = usersContext.GetUser(username);
            if (user != null)
            {
                var memUser = new MembershipUser("CustomMembershipProvider", username, user.UserID, user.EmailTxt,
                                                            string.Empty, string.Empty,
                                                            true, false, DateTime.MinValue,
                                                            DateTime.MinValue,
                                                            DateTime.MinValue,
                                                            DateTime.Now, DateTime.Now);
                return memUser;
            }   //  user.UserEmailAddress
            return null;
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>The user name associated with the specified e-mail address. If no match is found, return null.</returns>
        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        public override int MinRequiredPasswordLength
        {
            get { return 6; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user.</param>
        /// <returns>The new password for the specified user.</returns>
        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a lock so that the membership user can be validated.
        /// </summary>
        /// <param name="userName">The user to reset the password for.</param>
        /// <returns>true if the membership user was successfully unlocked; otherwise, false.</returns>
        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A MembershipUser object that represents the user to update and the updated information for the user.</param>
        public override void UpdateUser(MembershipUser user)
        {
            MembershipUser u = Membership.GetUser(user.ProviderUserKey);
            if (RequiresUniqueEmail && GetUserNameByEmail(user.Email) != string.Empty)
            {
                // status = MembershipCreateStatus.DuplicateEmail;
                return;
            }
            db_KISDEntities objDBEntities = new db_KISDEntities();
            var objuser = objDBEntities.Users.Find(user.ProviderUserKey);
            objuser.EmailTxt = user.Email;
            objDBEntities.SaveChanges();
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <returns>true if the specified username and password are valid; otherwise, false.</returns>
        public override bool ValidateUser(string username, string password)
        {
            var md5Hash = GetMd5Hash(password);

            using (var usersContext = new UsersContext())
            {
                var requiredUser = usersContext.GetUser(username, md5Hash);
                if (requiredUser != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Represents the abstract class from which all implementations of the MD5 hash algorithm inherit.
        /// </summary>
        /// <param name="value">The string value to decrypted in password.</param>
        /// <returns>The hash size for the MD5 algorithm is 128 bits.</returns>
        public static string GetMd5Hash(string value)
        {
            var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}