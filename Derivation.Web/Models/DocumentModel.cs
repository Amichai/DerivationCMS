using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Derivation.Web.Util;
using System.IO;

namespace Derivation.Web.Models {
    public class DocumentModel {
        [DebuggerDisplay("{TokenVal},{BreakingChar}")]
        public class Token {
            public Token(string val, char? breakingChar) {
                this.TokenVal = val;
                if (breakingChar.HasValue && breakingChar.Value == '\n') {
                    this.IsLineBreak = true;
                }
                this.BreakingChar = breakingChar;
                this.LinkedAnnotations = new List<int>();
            }

            public bool IsLineBreak { get; set; }
            public string TokenVal { get; set; }
            public char? BreakingChar { get; set; }
            public List<int> LinkedAnnotations { get; private set; }
            public string AsString {
                get {
                    return this.ToString();
                }
            }

            public override string ToString() {
                if(this.BreakingChar == null) {
                    return TokenVal;
                }
                return TokenVal + BreakingChar;
            }
        }

        private List<Token> tokenize(string body) {
            var toReturn = new List<Token>();
            int currentIndex = 0;
            while (true) {
                char? breakingChar;
                int newIndex = -1;
                nextBreak(body, currentIndex, out newIndex, out breakingChar);
                if (newIndex == -1) {
                    break;
                }
                int diff = newIndex - currentIndex;
                if (diff > 0) {
                    var t = new Token(body.Substring(currentIndex, diff), breakingChar.Value);
                    toReturn.Add(t);
                }
                else
                {
                    toReturn.Add(new Token("", breakingChar.Value));
                }
                currentIndex = newIndex + 1;
            }
            int lastSpan = body.Length - currentIndex;
            if (lastSpan != 0) {
                var lastToken = new Token(body.Substring(currentIndex, lastSpan), null);
                toReturn.Add(lastToken);
            }
            return toReturn;
        }

        public DocumentModel(string body) {
            this.Body = body;
            this.Info = new DocumentInfo();
            this.Tokens = this.tokenize(this.Body);
        }

        private void nextBreak(string body, int lastIndex, out int newIndex, out char? breakingChar) {
            newIndex = -1;
            breakingChar = null;
            foreach(var c in breakingChars) {
                int idx = body.IndexOf(c, lastIndex);
                if (idx != -1 && (idx < newIndex || newIndex == -1)) {
                    newIndex = idx;
                    breakingChar = c;
                }
            }
        }

        private List<char> breakingChars = new List<char>() { ' ', '\n' };
        public List<Token> Tokens { get; set; }

        public string Body { get; private set; }
        public DocumentInfo Info { get; set; }

        public bool IsArchived {
            get {
                return this.Info.IsArchived;
            }
            set {
                this.Info.IsArchived = value;
            }
        }

        public bool IsOpen {
            get {
                return this.Info.IsOpen;
            }
            set {
                this.Info.IsOpen = value;
            }
        }

        public bool IsPublic {
            get {
                return this.Info.IsPublic;
            }
            set {
                this.Info.IsPublic = value;
            }
        }

        public string Title {
            get {
                return this.Info.Title;
            }
            set {
                this.Info.Title = value;
            }
        }

        public string Author {
            get {
                return this.Info.Author;
            }
            set {
                this.Info.Author = value;
            }
        }

        public string Owner {
            get {
                return this.Info.Owner;
            }
            set {
                this.Info.Owner = value;
            }
        }

        public int AnnotationCount {
            get {
                return this.Info.AnnotationCount;
            }
            set {
                this.Info.AnnotationCount = value;
            }
        }

        public Guid Id {
            get {
                return this.Info.Id;
            }
            set {
                this.Info.Id = value;
            }
        }

        private int find(string[] array, string[] needle) {
            int needleLen = needle.Length;
            int index;
            int startIndex = 0;
            int sourceLength = array.Length;
            while (sourceLength >= needleLen) {
                index = Array.IndexOf(array, needle[0], startIndex, sourceLength - needleLen + 1);
                // if we did not find even the first element of the needls, then the search is failed
                if (index == -1)
                    return -1;

                int i, p;
                // check for needle
                for (i = 0, p = index; i < needleLen; i++, p++) {
                    if (array[p].Trim(toTrim) == needle[i].Trim(toTrim)) {
                        continue;
                    }
                    break;
                }

                if (i == needleLen) {
                    // needle was found
                    return index;
                }

                // continue to search for needle
                sourceLength -= (index - startIndex + 1);
                startIndex = index + 1;
            }
            return -1;
        }
        private static char[] toTrim = new char[] { ',', ':', '-', '.', ' ', ')', '(', ';' };
        internal TokenRange GetTokenRange(string annotationString) {
            if (string.IsNullOrWhiteSpace(annotationString)) {
                return null;
            }
            var tokens = this.tokenize(annotationString);
            var vals1 = this.Tokens.Select(i => i.TokenVal).ToArray();
            var vals2 = tokens.Select(i => i.TokenVal).ToArray();
            int idx = this.find(vals1, vals2);
            if (idx == -1) {
                return null;
            }
            return new TokenRange() {
                StartIdx = idx,
                Range = tokens.Count
            };
        }

        internal void ClearLinkedAnnotations() {
            this.Tokens.ForEach(i => i.LinkedAnnotations.Clear());
        }

        internal static DocumentModel FromDictionary(Dictionary<string, string> dict) {
            return new DocumentModel(dict["Body"]) {
                Id = Guid.Parse(dict["DocumentId"])
            };
        }

        internal static DocumentModel Random() {
            var text = File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/Placeholder.txt"));
            return new DocumentModel(text) {
                Author = "Test Author",
                Owner = "test",
                Title = "Paradise Lost",
                Id = Guid.NewGuid()
            };
        }
    }
}