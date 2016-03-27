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
        public AnnotatedDocument Get(Guid id) {
            DocumentModel doc = DynamoDBConnection.Instance.GetDocument(id);
            var user = IdentityUtil.GetCurrentUser();
            List<AnnotationModel> annotations = DynamoDBConnection.Instance.GetAnnotations(id, doc).Where(i => !i.IsArchived).ToList();
            return new AnnotatedDocument(annotations.OrderBy(i => i.Ord).ToList(),
                doc);
        }
    }
}
