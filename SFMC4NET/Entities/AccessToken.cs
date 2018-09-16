using System;

namespace SFMC4NET.Entities
{
    public class AccessToken
    {
        public string Token { get; private set; }
        public DateTime TokenExpiracy { get; private set; }

        public AccessToken(string token, string expiracy)
        {
            Token = token;
            TokenExpiracy = DateTime.Now.AddSeconds(double.Parse(expiracy));
        }

        public bool IsValid
        {
            get
            {
                return !((TokenExpiracy - DateTime.Now).TotalMinutes <= 1);
            }
        }
    }
}
