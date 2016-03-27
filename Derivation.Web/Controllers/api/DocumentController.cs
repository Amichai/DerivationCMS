using Derivation.Web.Data;
using Derivation.Web.Models;
using Derivation.Web.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Derivation.Web.Models;

namespace Derivation.Web.Controllers.api
{
    public class DocumentController : ApiController
    {
        // GET: api/Document
        public IEnumerable<DocumentInfo> Get() {
            var currentUser = IdentityUtil.GetCurrentUser();
            var documents = DynamoDBConnection.Instance.GetUserDocuments(currentUser.UserId)
                .Where(i => !i.IsArchived).ToList();
            documents.ForEach(i => {
                if (i.Owner == currentUser.UserId) {
                    i.IsOwnedByMe = true;
                }
            });
            return documents;
        }

        public DocumentModel Get(Guid id) {
            //TODO: Check user permissions
            return DynamoDBConnection.Instance.GetDocument(id);
        }

        // POST: api/Document
        public DocumentModel Post([FromBody]DocumentModel doc) {
            doc.Owner = IdentityUtil.GetCurrentUser().UserId;
            doc.Id = Guid.NewGuid();
            bool success = DynamoDBConnection.Instance.AddDocument(doc);
            return doc;
        }

        // PUT: api/Document/5
        public void Put([FromBody]DocumentModel doc) {
            DynamoDBConnection.Instance.AddDocument(doc);
        }

        // DELETE: api/Document/5
        public void Delete(Guid id) {
            //TODO: check user permissions
            //DynamoDBConnection.Instance.DeleteDocument(id, currentUser.UserId);
            DynamoDBConnection.Instance.ArchiveDocument(id);
        }
    }
}
