using KISD.Common.Interfaces;
using System;
using System.Web.Security;

namespace KISD.Common.Services
{
    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        /// <summary>
        /// This method override the functionality of the FormsAuthentication to login.
        /// </summary>
        /// <param name="userName">Username of login user.</param>
        /// <param name="createPersistentCookie"></param>
        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentException("Value cannot be null or empty.", "userName");

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }
        /// <summary>
        /// This method override the functionality of the FormsAuthentication to Logout.
        /// </summary>
        public void SignOut()
        {
            FormsAuthentication.SignOut();
        }
    }
} 