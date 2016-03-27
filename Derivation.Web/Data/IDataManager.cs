using Newtonsoft.Json.Linq;
using System;
using Derivation.Web.Models;
using System.Collections.Generic;
namespace Derivation.Web.Data {
    public interface IDataManager {
        bool UpdateUser(UserModel user);
        List<UserModel> GetAllUsers(int limit);
        void AddNewUser(string userId, JObject value);
        Dictionary<string, string> GetUser(string userId);
        DocumentModel GetDocument(Guid id);
        IEnumerable<DocumentInfo> GetUserDocuments(string userId);
        bool AddDocument(DocumentModel doc);
        void AddAnnotationAndLinkToUser(NewAnnotationModel newAnnotation, string userId);
        List<AnnotationModel> GetAnnotations(Guid documentId, DocumentModel doc);
        AnnotationDataModel GetAnnotation(Guid id);
        void ArchiveDocument(Guid docId);
        void UpdateAnnotation(UpdateAnnotationModel annotation);
        void ArchiveAnnotation(Guid id);

        IEnumerable<AnnotationDataModel> GetUserAnnotations(string userId);
        void AddDocumentPermission(Guid documentId, string user);
        void RemoveDocumentPermission(Guid docId, string user);

        void MakePublic(Guid docId, bool makePublic);

        void MakeOpen(Guid docId, bool state);
        void UserDocumentPermissions(Guid id, string userId, out bool canView, out bool canEdit);
    }
}
