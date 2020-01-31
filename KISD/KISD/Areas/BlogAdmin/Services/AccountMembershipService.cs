using KISD.Areas.BlogAdmin.Interfaces;
using System;
using System.Web.Security;

namespace KISD.Areas.BlogAdmin.Services
{
    public class AccountMembershipService: IMembershipService
    {
        public MembershipProvider _provider;
        /// <summary>
        /// Constructor of Membership Provider to intialize the Provider Service.
        /// </summary>
        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Providers["BlogCustomMembershipProvider"];
        }
        /// <summary>
        ///    Gets the minimum length required for a password.
        ///Returns The minimum length required for a password.
        /// </summary>
        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }
        /// <summary>
        /// This method override the functionality of the MembershipProvider to validate the user.
        /// </summary>
        /// <param name="userName">Username of login user.</param>
        /// <param name="password">Password of login user.</param>
        /// <returns>Check the user with the credentials,if Valid then return True else false. </returns>
        public bool ValidateUser(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            //var membershipProvider2 = Membership.Providers["MembershipProvider2"];
            return _provider.ValidateUser(userName, password);
        }
        /// <summary>
        /// This method override the functionality of the MembershipProvider to add new the user.
        /// </summary>
        /// <param name="userName">Username of login user.</param>
        /// <param name="password">Password of login user.</param>
        /// <param name="email">Email of login user.</param>
        /// <returns>Create a new User and Return enum value with the status. </returns>
        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (string.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");

            MembershipCreateStatus status;
            _provider.CreateUser(userName, password, email, null, null, true, null, out status);
            return status;
        }

        /// <summary>
        /// This method override the functionality of the MembershipProvider to change the password.
        /// </summary>
        /// <param name="userName">Username of login user.</param>
        /// <param name="oldPassword">Old Password of login user.</param>
        /// <param name="newPassword">New Password of login user.</param>
        /// <returns> Return bool value. </returns>
        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (string.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            if (string.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }
    }
}