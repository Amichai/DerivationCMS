using Derivation.Web.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Derivation.Web.Data {
    public class FakeDataManager : IDataManager {
        public FakeDataManager() {
            this.documents = Enumerable.Range(0, 10).Select(i => DocumentInfo.Random()).ToList();
        }

        public void MakeOpen(Guid docId, bool state) {
            throw new NotImplementedException();
        }

        private Dictionary<string, JObject> userIdToUser = new Dictionary<string, JObject>() {
            { "test", JObject.Parse(@"{UserInfo: {
FirstName:"""", LastName: """", Created: 635831905773640372, UserId: ""test"", Role: ""user"", Password: ""test""
} }")},
    { "Amichai", JObject.Parse(@"{UserInfo: {
FirstName:"""", LastName: """", Created: 635831905773640372, UserId: ""test"", Role: ""user"", Password: ""test""
} }")}
        };

        public void AddNewUser(string userId, JObject value) {
            this.userIdToUser[userId] = value;
        }

        public List<UserModel> GetAllUsers(int limit) {
            return this.userIdToUser.Values.Select(i => UserModel.FromJson(i)).ToList();
        }

        public void MakePublic(Guid docId, bool makePublic) {
            throw new NotImplementedException();
        }

        public IEnumerable<AnnotationDataModel> GetUserAnnotations(string userId) {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetUser(string userId) {
            var obj = this.userIdToUser[userId];
            var toReturn = new Dictionary<string, string>();
            foreach (var p in obj.Properties()) {
                toReturn[p.Name] = p.Value.ToString();
            }
            return toReturn;
        }

        public string GetUserId(string sessionId) {
            throw new NotImplementedException();
        }

        public JObject RetrieveUser(string userId) {
            throw new NotImplementedException();
        }

        public bool UpdateUser(UserModel user) {
            throw new NotImplementedException();
        }

        private List<DocumentInfo> documents;

        public IEnumerable<DocumentInfo> GetUserDocuments(string userId) {
            return this.documents;
        }

        private Dictionary<Guid, DocumentModel> idToDocument = new Dictionary<Guid, DocumentModel>();

        public bool AddDocument(DocumentModel doc) {
            this.documents.Add(doc.Info);
            this.idToDocument[doc.Id] = doc;
            return true;
        }

        public void AddDocumentPermission(Guid docId, string user) {
            throw new NotImplementedException();
        }

        public void RemoveDocumentPermission(Guid docId, string user) {
            throw new NotImplementedException();
        }

        public void AddAnnotationAndLinkToUser(NewAnnotationModel newAnnotation, string userId) {
            throw new NotImplementedException();
        }

        private static Random rand = new Random();

        public List<AnnotationModel> GetAnnotations(Guid documentId, DocumentModel doc) {
            if (this.annotations != null) {
                return annotations.Values.ToList();
            }

            int annotationCount = rand.Next(10, 50);
            var toReturn = new List<AnnotationModel>();
            for (int i = 0; i < annotationCount; i++) {
                toReturn.Add(AnnotationModel.Random(doc));
            }
            this.annotations = toReturn.ToDictionary(i => i.Id, i => i);
            return this.annotations.Values.ToList();
        }

        public DocumentModel GetDocument(Guid id) {
            DocumentModel toReturn;
            if (this.idToDocument.TryGetValue(id, out toReturn)) {
                return toReturn;
            }
            return DocumentModel.Random();
        }

        public void ArchiveDocument(Guid docId) {
            var d = this.GetDocument(docId);
            d.IsArchived = true;
        }

        private Dictionary<Guid, AnnotationModel> annotations = null;

        public void UpdateAnnotation(UpdateAnnotationModel annotation) {
            annotations[annotation.Id].Body = annotation.Body;
        }


        public AnnotationDataModel GetAnnotation(Guid id) {
            throw new NotImplementedException();
        }

        public void ArchiveAnnotation(Guid id) {
            throw new NotImplementedException();
        }

        public void UserDocumentPermissions(Guid id, string userId, out bool canView, out bool canEdit) {
            throw new NotImplementedException();
        }
    }
}