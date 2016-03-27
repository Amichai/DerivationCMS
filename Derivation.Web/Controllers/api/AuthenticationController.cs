using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Web.Http;
using System.Text;
using System.Web.Security;
using System.Web;
using log4net;
using System.Threading.Tasks;
using Derivation.Web.Models;
using Derivation.Web.Data;

namespace Derivation.Web.Controllers.api
{
    public class AuthenticationController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Authenticates a user and sets an authentication token
        /// </summary>
        [HttpPost]
        [Route("api/Authentication/Authenticate")]
        public HttpResponseMessage Authenticate() {
            var credentials = Request.Content.ReadAsStringAsync().Result;
            var postData = JObject.Parse(credentials);
            var username = postData["Username"].ToString().Trim();
            var password = postData["Password"].ToString().Trim();
            var match = DynamoDBConnection.Instance.GetUser(username);
            if (match == null) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unrecognized username or password");
            }
            var data = JObject.Parse(match["UserInfo"]);
            var hashedPassword = data["Password"].ToString();
            bool authenticated;
            if (hashedPassword != "test") {
                byte[] charArray = hashedPassword.Select(i => (byte)i).ToArray();
                var passwordHash = new PasswordHash(charArray);
                authenticated = passwordHash.Verify(password);
            } else {
                authenticated = true;
            }
            if (authenticated) {
                var toReturn = new HttpResponseMessage(HttpStatusCode.OK);
                FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(username, true, 525600);
                var sessionKey = FormsAuthentication.Encrypt(ticket);
                toReturn.Headers.Add("Set-Cookie", string.Format("session_id={0}; Path=/", sessionKey));
                toReturn.Headers.Add("Set-Cookie", string.Format("user_id={0}; Path=/", username));
                toReturn.Content = new StringContent(sessionKey.ToString());
                return toReturn;
            } else {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unrecognized username or password");
            }
        }

        public static bool IsSessionIdValid(HttpRequestBase request) {
            var sessionId = request.Cookies["session_id"];
            var userId = request.Cookies["user_id"];
            if (sessionId == null || userId == null) {
                return false;
            }
            try {
                var ticket = FormsAuthentication.Decrypt(sessionId.Value);
                return !ticket.Expired && ticket.Name == userId.Value;
            } catch (FormatException ex) {
                log.WarnFormat("Failed to parse token: {0}", sessionId.Value);
                return false;
            }
        }
    }
}
