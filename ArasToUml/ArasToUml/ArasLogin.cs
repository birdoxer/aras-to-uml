using System;
using Aras.IOM;

namespace ArasToUml
{
    internal class ArasLogin
    {
        internal ArasLogin(string url, string db, string user, string passWord)
        {
            ServerConnection = IomFactory.CreateHttpServerConnection(url, db, user, passWord);
            Item loginResult = ServerConnection.Login();
            if (loginResult.isError()) throw new Exception(loginResult.ToString());
            Innovator = IomFactory.CreateInnovator(ServerConnection);
        }

        internal Innovator Innovator { get; }

        private HttpServerConnection ServerConnection { get; }
    }
}