using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace Derivation.Web.Models {
    public class UserModel {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public long Created { get; set; }
        public bool IsAdministrator {
            get {
                return this.Role == "admin";
            }
        }

        public bool IsManager {
            get {
                return this.Role == "manager";
            }
        }

        internal static UserModel Create(string username, string password) {
            var pass = new PasswordHash(password);
            var arr = pass.ToArray();
            return new UserModel() {
                UserId = username,
                Password = string.Concat(arr.Select(i => (char)i)),
                Role = "user",
                Created = DateTime.Now.Ticks
            };
        }

        public string CreatedString {
            get {
                return new DateTime(this.Created).ToShortDateString();
            }
        }

        public JObject ToJson() {
            JObject toReturn = new JObject();
            toReturn["UserId"] = this.UserId;
            toReturn["FirstName"] = this.FirstName;
            toReturn["LastName"] = this.LastName;
            toReturn["Password"] = this.Password;
            toReturn["Role"] = this.Role;
            toReturn["Created"] = this.Created.ToString();
            return toReturn;
        }

        internal static UserModel FromJson(JObject data) {
            string firstName = "";
            string lastName = "";
            if (data["FirstName"].HasValues) {
                firstName = data["FirstName"].Value<string>();
            }
            if (data["LastName"].HasValues) {
                lastName = data["LastName"].Value<string>();
            }
            return new UserModel() {
                UserId = data["UserId"].Value<string>(),
                FirstName = firstName,
                LastName = lastName,
                Created = data["Created"].Value<long>(),
                Role = data["Role"].Value<string>()
            };
        }
    }
}