using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Security;
using Derivation.Web.Data;
using Derivation.Web.Models;

namespace Derivation.Web.Util {
    public static class IdentityUtil {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static IDataManager DataManager = DynamoDBConnection.Instance;

        public static UserModel GetCurrentUser() {
            var session = HttpContext.Current.Request.Cookies["session_id"];
            if (session == null) {
                return null;
            }
            string sessionId = session.Value;
            try {
                var ticket = FormsAuthentication.Decrypt(sessionId);
                var userId = ticket.Name;
                var user = DataManager.GetUser(userId);
                var userData = JObject.Parse(user["UserInfo"]);
                return UserModel.FromJson(userData);
            } catch (FormatException ex) {
                log.WarnFormat("Failed to parse token: {0}", sessionId);
                return null;
            }
        }
    }
}