using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Derivation.Web.Models {
    public class TokenizedAnnotation {
        public TokenizedAnnotation() {
            this.Tokens = new List<annotationUnit>();
            this.CharLength = 0;
        }
        public List<annotationUnit> Tokens { get; private set; }
        public int CharLength { get; private set; }

        public void Add(string val, bool linkedText = false) {
            this.Tokens.Add(new annotationUnit(val, linkedText));
            this.CharLength += val.Length;
        }

        public class annotationUnit {
            public annotationUnit(string val, bool linkedText = false) {
                this.Val = val.Trim('`');
                this.linkedText = linkedText;
                this.Type = linkedText ? "LinkedText" : "";
            }
            public string Type { get; set; }
            public string Val { get; set; }
            internal bool linkedText;
        }
    }
}