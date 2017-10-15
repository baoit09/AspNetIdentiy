using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web;
using Thinktecture.IdentityModel.Tokens;

namespace AspNetIdentity.WebApi.Providers
{
    public class CustomJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
    
        private readonly string _issuer = string.Empty;
 
        public CustomJwtFormat(string issuer)
        {
            _issuer = issuer;
        }
 
        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
 
            // As stated previously, this API servers as Authorization Server and Resource Server at the same time, so we are fixing 
            // the Id and Secret in web.config. 
            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            string symmetricKeyAsBase64 = ConfigurationManager.AppSettings["as:AudienceSecret"];
 
            var keyByteArray = TextEncodings.Base64Url.Decode(symmetricKeyAsBase64);
 
            // Use HMAC256 (called SHA256) algorithm to hash/sign/generate the digital signature.
            var signingKey = new HmacSigningCredentials(keyByteArray);
 
            // Prepare data for payload: claims, date issued, date expired, issuer, audiences.
            var issued = data.Properties.IssuedUtc;
            var expires = data.Properties.ExpiresUtc;

            // Create a JSON format of token 
            var token = new JwtSecurityToken(_issuer, audienceId, data.Identity.Claims, issued.Value.UtcDateTime, expires.Value.UtcDateTime, signingKey);
 
            // Transform the JSON format token to JWT format token. (header.payload,signature)
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.WriteToken(token);
 
            // The requester will receive a signed token, which contains the claims of authenticated user and this access token is intended to a certaine audience.
            return jwt;
        }
 
        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}