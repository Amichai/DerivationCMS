using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Derivation.Web.Models {
    public class AnnotationModel {
        private const int PREVIEW_LENGTH = 200;

        public AnnotationModel(string author, string body, DocumentModel fullText, bool isArchived) {
            this.Author = author;
            this.Body = body;
            if (this.Body.Length <= PREVIEW_LENGTH) {
                this.PreviewText = this.Body;
            } else {
                this.PreviewText = this.Body.Substring(0, 200) + "...";
            }
            this.setAnnotationBodyUnits(fullText);
            this.IsStarred = false;
            this.IsArchived = isArchived;
        }

        public bool IsArchived { get; set; }
        public bool IsStarred { get; set; }

        public bool IsPreviewCutoff {
            get {
                return this.AnnotationBodyUnits.CharLength - 1 > this.AnnotationPreviewUnits.CharLength;
            }
        }

        private void setAnnotationBodyUnits(DocumentModel fullText) {
            if (fullText == null) {
                throw new ArgumentNullException("fullText");
            }
            this.AnnotationBodyUnits = new TokenizedAnnotation();
            var indices = this.backTickIndices();
            int lastIdx = 0;
            for (int i = 0; i < indices.Count; i += 2) {
                if (i == indices.Count - 1) {
                    break;
                }
                int idx1 = indices[i];
                int idx2 = indices[i + 1];
                string prefix = this.Body.Substring(lastIdx, idx1);
                this.AnnotationBodyUnits.Add(prefix);
                string annotationString = this.Body.Substring(idx1, idx2).Trim('`');
                this.TokenRange = fullText.GetTokenRange(annotationString);

                this.AnnotationBodyUnits.Add(annotationString, linkedText:true);
                lastIdx = idx2;
            }
            string suffix = string.Concat(this.Body.Skip(lastIdx));
            this.AnnotationBodyUnits.Add(suffix);
            this.AnnotationPreviewUnits = new TokenizedAnnotation();

            foreach (var unit in this.AnnotationBodyUnits.Tokens) {
                int curLength = 0;
                int toTake = Math.Min(unit.Val.Length, PREVIEW_LENGTH - curLength);
                string val = unit.Val.Substring(0, toTake);
                this.AnnotationPreviewUnits.Add(val, unit.linkedText);
                curLength += val.Length;
                if (curLength > PREVIEW_LENGTH) {
                    continue;
                }
            }
        }

        public int Ord {
            get {
                if (this.TokenRange == null) {
                    return -1;
                }
                return this.TokenRange.StartIdx;
            }
        }
        public TokenRange TokenRange { get; private set; }
        public string Author { get; private set; }
        public string Body { get; set; }
        public string PreviewText { get; set; }
        public Guid Id { get; private set; }

        private List<int> backTickIndices() {
            List<int> toReturn = new List<int>();
            int idx = -1;
            while (true) {
                idx = this.Body.IndexOf('`', idx + 1);
                if (idx == -1) {
                    break;
                }
                toReturn.Add(idx);
            }
            return toReturn;
        }

        public List<string> MyProperty { get; set; }
        public TokenizedAnnotation AnnotationBodyUnits { get; private set; }
        public TokenizedAnnotation AnnotationPreviewUnits { get; private set; }


        internal static AnnotationModel FromDictionary(Dictionary<string, string> dict, DocumentModel doc) {
            bool isArchived = false;
            if (dict.ContainsKey("IsArchived")) {
                isArchived = bool.Parse(dict["IsArchived"]);
            }
            var toReturn = new AnnotationModel(dict["Author"], dict["Body"], doc, isArchived);
            toReturn.Id = Guid.Parse(dict["AnnotationId"]);
            return toReturn;
        }

        private static Random rand = new Random();

        internal static AnnotationModel Random(DocumentModel doc) {
            int maxTokensToTake = Math.Min(20, doc.Tokens.Count);
            var text = File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data/Placeholder2.txt"));
            var tokenCount = doc.Tokens.Count;
            var startIdx = rand.Next(tokenCount - maxTokensToTake);
            int toTake = rand.Next(maxTokensToTake);
            var quoted = string.Concat(doc.Tokens.Skip(startIdx).Take(toTake).Select(i => i.AsString));
            var textLength = text.Length;
            int textStart = rand.Next(textLength);
            int textToTake = rand.Next(1000);
            var body = string.Format("`{0}`", quoted) + 
                string.Concat(text.Skip(textStart).Take(Math.Min(textToTake, textLength - textStart)));
            var toReturn = new AnnotationModel("test", body, doc, isArchived:false);
            toReturn.Id = Guid.NewGuid();
            return toReturn;
        }
    }
}