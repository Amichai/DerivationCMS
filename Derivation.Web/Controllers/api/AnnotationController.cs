using Derivation.Web.Data;
using Derivation.Web.Models;
using Derivation.Web.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Derivation.Web.Controllers.api {
    public class AnnotationController : ApiController {
        public void Post([FromBody]NewAnnotationModel newAnnotation) {
            var current = IdentityUtil.GetCurrentUser();
            newAnnotation.AnnotationId = Guid.NewGuid();
            DynamoDBConnection.Instance.AddAnnotationAndLinkToUser(newAnnotation, current.UserId);
        }

        public void Put([FromBody] UpdateAnnotationModel annotation) {
            DynamoDBConnection.Instance.UpdateAnnotation(annotation);
        }

        public bool Delete(Guid id) {
            var current = IdentityUtil.GetCurrentUser();
            if(!current.IsAdministrator) {
                var annotation = DynamoDBConnection.Instance.GetAnnotation(id);
                if (annotation.Author != current.UserId) {
                    return false;
                }
            }
            DynamoDBConnection.Instance.ArchiveAnnotation(id);
            return true;
        }
    }
}
