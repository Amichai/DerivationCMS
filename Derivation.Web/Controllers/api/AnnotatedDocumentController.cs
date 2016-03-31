using Derivation.Web.Data;
using Derivation.Web.Models;
using Derivation.Web.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Derivation.Web.Controllers.api {
    public class AnnotatedDocumentController : ApiController {
        public DerivationModel Get(Guid id) {
            DerivationModel doc = DynamoDBConnection.Instance.GetDerivation(id);
            var user = IdentityUtil.GetCurrentUser();
            return new DerivationModel();
        }
    }
}
