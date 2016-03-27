using Derivation.Web.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Derivation.Web.Models {
    public class DocumentInfo {
        public DocumentInfo() {
            this.IsArchived = false;
            this.Permissioned = new List<string>();
        }

        public bool IsPublic { get; set; }
        public List<string> Permissioned { get; set; }
        public string PermissionedString {
            get {
                return string.Join(", ", this.Permissioned);
            }
        }

        public string Title { get; set; }
        public string Owner { get; set; }
        public int AnnotationCount { get; set; }
        public string Author { get; set; }
        public bool IsArchived { get; set; }
        public Guid Id { get; set; }

        internal static DocumentInfo FromJson(string p) {
            throw new NotImplementedException();
        }

        internal static DocumentInfo FromDictionary(Dictionary<string, string> dict) {
            string author = "";
            if (dict.ContainsKey("Author")) {
                author = dict["Author"];
            }
            var permissioned = JArray.Parse(dict["Permissioned"]);
            return new DocumentInfo() {
                Title = dict["Title"],
                Owner = dict["Owner"],
                Author = author,
                AnnotationCount = int.Parse(dict["AnnotationCount"]),
                Id = Guid.Parse(dict["DocumentId"]),
                IsArchived = bool.Parse(dict["IsArchived"]),
                IsPublic = bool.Parse(dict["IsPublic"]),
                IsOpen = bool.Parse(dict["IsOpen"]),
                Permissioned = permissioned.Select(i => i.ToString()).ToList()
            };
        }

        private static Random rand = new Random();

        internal static DocumentInfo Random() {
            return new DocumentInfo() {
                Id = Guid.NewGuid(),
                Owner = "test",
                Title = "Title_" + rand.Next(0, 100),
                Author = "Author_" + rand.Next(0, 100),
            };
        }

        public bool IsOwnedByMe { get; set; }
        public bool IsOpen { get; set; }
    }
}