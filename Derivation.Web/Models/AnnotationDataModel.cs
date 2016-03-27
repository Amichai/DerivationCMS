using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Derivation.Web.Models {
    public class AnnotationDataModel {
        private const int PREVIEW_LENGTH = 200;

        public AnnotationDataModel(string body, string author, Guid id) {
            this.Body = body;
            this.Author = author;
            this.Id = id;
            if (this.Body.Length <= PREVIEW_LENGTH) {
                this.PreviewText = this.Body;
            } else {
                this.PreviewText = this.Body.Substring(0, 200) + "...";
            }
        }
        public string Body { get; private set; }
        public string Author { get; private set; }
        public Guid Id { get; private set; }
        public string PreviewText { get; private set; }

        internal static AnnotationDataModel FromDictionary(Dictionary<string, string> dict) {
            string body = dict["Body"];
            string author = dict["Author"];
            Guid id = Guid.Parse(dict["AnnotationId"]);
            return new AnnotationDataModel(body, author, id);
        }
    }
}