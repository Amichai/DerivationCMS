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
            return true;
        }

        public bool Delete(string docId, string user) {
            return true;
        }

        [Route("api/MakePublic")]
        public bool MakePublic(Guid docId, bool state) {
            return true;
        }

        [Route("api/MakeOpen")]
        public bool MakeOpen(Guid docId, bool state) {
            return true;
        }
    }
}
