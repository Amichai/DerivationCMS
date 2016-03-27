using Derivation.Web.Data;
using Derivation.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Derivation.Web.Controllers.api
{
    public class PermissionsController : ApiController
    {
        public bool Post([FromBody]NewDocumentPermission newPermission) {
            DynamoDBConnection.Instance.AddDocumentPermission(newPermission.DocumentId, newPermission.User);
            return true;
        }

        public bool Delete(string docId, string user) {
            DynamoDBConnection.Instance.RemoveDocumentPermission(Guid.Parse(docId), user);
            return true;
        }

        [Route("api/MakePublic")]
        public bool MakePublic(Guid docId, bool state) {
            DynamoDBConnection.Instance.MakePublic(docId, state);
            return true;
        }

        [Route("api/MakeOpen")]
        public bool MakeOpen(Guid docId, bool state) {
            DynamoDBConnection.Instance.MakeOpen(docId, state);
            return true;
        }
    }
}
