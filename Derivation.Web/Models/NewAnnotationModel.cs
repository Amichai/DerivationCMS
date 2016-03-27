using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Derivation.Web.Models {
    public class NewAnnotationModel {
        public NewAnnotationModel(string body, Guid documentId, Guid annotationId) {
            this.Body = body;
            this.DocumentId = documentId;
            this.AnnotationId = annotationId;
        }
        ///TODO: rename this to unlinked annotation model
        ///remove the update annotation model object
        public string Body { get; private set; }
        public Guid DocumentId { get; private set; }
        public Guid AnnotationId { get; set; }
    }
}