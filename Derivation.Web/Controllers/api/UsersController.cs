using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Security;
using Derivation.Web.Data;
using Derivation.Web.Models;
using Derivation.Web.Util;

namespace Derivation.Web.Controllers.api {
    public class UsersController : ApiController {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public UsersController() {
            this.dataManager = DynamoDBConnection.Instance;
            this.getCurrentUser = IdentityUtil.GetCurrentUser;
        }

        private DynamoDBConnection dataManager;
        private Func<UserModel> getCurrentUser;

        public UsersController(DynamoDBConnection dataManager) {
            this.dataManager = dataManager;
            this.getCurrentUser = () => new UserModel() {
                UserId = "TestUser",
                Role = "admin"
            };
        }

        /// <summary>
        /// Registers a new user account on the site
        /// </summary>
        [HttpPost]
        public HttpResponseMessage Post([FromBody] RegisterUserModel model) {
            var username = model.Username.Trim();
            var password = model.Password.Trim();
            var record = UserModel.Create(username, password);
            var recordJson = record.ToJson();
            if (this.dataManager.GetUser(record.UserId) != null) {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Username already exists");
            }
            this.dataManager.AddNewUser(record.UserId, recordJson);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        /// <summary>
        /// Gets a list of all registered users on the site. Requires admin or manager roles.
        /// </summary>
        public List<UserModel> Get() {
            var currentUser = this.getCurrentUser();
            if (!currentUser.IsManager && !currentUser.IsAdministrator) {
                return null;
            }
            List<UserModel> allUsers = this.dataManager.GetAllUsers(1000);
            return allUsers;
        }

        /// <summary>
        /// Updates information for a particular user
        /// </summary>
        [Route("api/Users/{userId}")]
        public bool Put(string userId, [FromBody] UserModel user) {
            if (userId != user.UserId) {
                log.Warn("Inconsistent user id");
                return false;
            }
            bool success = this.dataManager.UpdateUser(user);
            return success;
        }

        /// <summary>
        /// Gets information about the currently authenticated user
        /// </summary>
        /// <returns>A user model</returns>
        [Route("api/Users/GetCurrentUser")]
        public UserModel GetCurrentUser() {
            return this.getCurrentUser();
        }
    }
}
