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

        // POST: api/Derivation
        public DerivationModel Post(Guid id, [FromBody]DerivationModel doc)
        {
            //doc.Owner = IdentityUtil.GetCurrentUser().UserId;
            //doc.Id = Guid.NewGuid();
            //bool success = DynamoDBConnection.Instance.AddDerivation(doc);
            doc.Id = id;
            DynamoDBConnection.Instance.AddDerivation(doc);
            return doc;

        }

        public DerivationModel Get(Guid id)
        {
            return DynamoDBConnection.Instance.GetDerivation(id);
        }

        public IEnumerable<DerivationModel> Get()
        {
            var currentUser = IdentityUtil.GetCurrentUser();
            var documents = DynamoDBConnection.Instance.GetAllDerivations(100)
                .Where(i => !i.IsArchived).ToList();

            return documents;
        }

        public void Put([FromBody]DerivationModel doc)
        {
            DynamoDBConnection.Instance.AddDerivation(doc);
        }

        public void Delete(Guid id)
        {
            //TODO: check user permissions
            //DynamoDBConnection.Instance.DeleteDocument(id, currentUser.UserId);
            DynamoDBConnection.Instance.ArchiveDocument(id);
        }
    }
}
