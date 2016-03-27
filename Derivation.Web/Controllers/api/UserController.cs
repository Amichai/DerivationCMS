using Derivation.Web.Data;
using Derivation.Web.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Derivation.Web.Controllers.api
{
    public class UserController : ApiController {
        [Route("api/User/{userId}")]
        public ProfileModel Get(string userId) {
            var db = DynamoDBConnection.Instance;
            var userDict = db.GetUser(userId);
            if (userDict == null) {
                return null;
            }
            var json = JObject.Parse(userDict["UserInfo"]);
            var user = UserModel.FromJson(json);
            var docs = db.GetUserDocuments(userId);
            var annotations = db.GetUserAnnotations(userId);
            return new ProfileModel() {
                Created = new DateTime(user.Created),
                CreatedString = user.CreatedString,
                UserId = userId,
                RecentAnnotations = annotations.ToList(),
                Documents = docs.ToList()
            };
        }
    }
}
