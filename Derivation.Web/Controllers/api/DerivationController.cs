using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Derivation.Web.Data;
using Derivation.Web.Models;
using Derivation.Web.Util;

namespace Derivation.Web.Controllers.api
{
    public class DerivationController : ApiController
    {
        // POST: api/Derivation
        public DerivationModel Post([FromBody]DerivationModel doc)
        {
            doc.Owner = IdentityUtil.GetCurrentUser().UserId;
            doc.Id = Guid.NewGuid();
            bool success = DynamoDBConnection.Instance.AddDerivation(doc);
            return doc;
        }

        public IEnumerable<DerivationModel> Get()
        {
            var currentUser = IdentityUtil.GetCurrentUser();
            var documents = DynamoDBConnection.Instance.GetAllDerivations(100)
                .Where(i => !i.IsArchived).ToList();

            return documents;
        }
    }
}
