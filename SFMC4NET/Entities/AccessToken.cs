using System;

namespace SFMC4NET.Entities
{
    public class AccessToken
    {
        private bool userProvidedToken = false;

        public string Token { get; private set; }
        public DateTime TokenExpiracy { get; private set; }

        public AccessToken(string token, string expiracy)
        {
            Token = token;
            TokenExpiracy = DateTime.Now.AddSeconds(double.Parse(expiracy));
        }

        public AccessToken(string token)
        {
            Token = token;
            userProvidedToken = true;
        }

        public bool IsValid
        {
            get
            {
                if(!userProvidedToken)
                    return !((TokenExpiracy - DateTime.Now).TotalMinutes <= 1);

                return true;
            }
        }
    }
}
