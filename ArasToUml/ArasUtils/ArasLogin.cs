using System.Security.Authentication;
using Aras.IOM;

namespace ArasToUml.ArasUtils
{
    /// <summary>
    ///     Class that establishes a server connection to the Aras instance.
    /// </summary>
    internal class ArasLogin
    {
        /// <summary>
        ///  The only constructor for the class.
        /// </summary>
        /// <remarks>
        ///     The constructor itself performs the Aras login and stores the resulting Innovator object in a class
        ///     property.
        /// </remarks>
        /// <param name="url">The URL to the Aras instance.</param>
        /// <param name="db">The database name from which to extract the data model.</param>
        /// <param name="user">The user with which to login to Aras.</param>
        /// <param name="passWord">The password for <paramref name="user"/>.</param>
        /// <exception cref="AuthenticationException">Thrown if no server connection can be established.</exception>
        internal ArasLogin(string url, string db, string user, string passWord)
        {
            ServerConnection = IomFactory.CreateHttpServerConnection(url, db, user, passWord);
            var loginResult = ServerConnection.Login();
            if (loginResult.isError()) throw new AuthenticationException(loginResult.ToString());
            Innovator = IomFactory.CreateInnovator(ServerConnection);
        }

        internal Innovator Innovator { get; }

        private HttpServerConnection ServerConnection { get; }

        /// <summary>
        ///     Terminates the current server connection to the Innovator instance.
        /// </summary>
        internal void LogOut()
        {
            ServerConnection.Logout();
        }
    }
}