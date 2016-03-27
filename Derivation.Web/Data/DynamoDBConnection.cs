using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Derivation.Web.Models;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace Derivation.Web.Data {
    public class DynamoDBConnection : IDataManager {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IAmazonDynamoDB client;

        private DynamoDBConnection(RegionEndpoint endpoint = null) {
            if (endpoint == null) {
                endpoint = RegionEndpoint.USEast1;
            }
            client = new AmazonDynamoDBClient(endpoint);
        }

        public static IDataManager Instance = new DynamoDBConnection();
        //public static IDataManager Instance = new FakeDataManager();

        public void MakePublic(Guid docId, bool state) {
            var info = this.GetDocumentInfo(docId);
            if(info.IsPublic != state) {
                info.IsPublic = state;
            }
            this.setDocumentInfo(info);
        }

        public void MakeOpen(Guid docId, bool state) {
            var info = this.GetDocumentInfo(docId);
            if (info.IsOpen != state) {
                info.IsOpen = state;
            }
            this.setDocumentInfo(info);
        }

        private void setDocumentInfo(string title, string owner, int annotationCount, Guid id,
            string author, bool isArchived, bool isPublic, bool isOpen, List<string> permissioned) {
            var attributes = new List<TableAttribute>();
            attributes.Add(new TableAttribute("Title", title));
            attributes.Add(new TableAttribute("Owner", owner));
            attributes.Add(new TableAttribute("AnnotationCount", annotationCount.ToString()));
            attributes.Add(new TableAttribute("Id", id.ToString()));
            if (!string.IsNullOrWhiteSpace(author)) {
                attributes.Add(new TableAttribute("Author", author));
            }
            attributes.Add(new TableAttribute("IsArchived", isArchived.ToString()));
            attributes.Add(new TableAttribute("IsPublic", isPublic.ToString()));
            attributes.Add(new TableAttribute("IsOpen", isOpen.ToString()));
            JArray newArr = new JArray();
            permissioned.ForEach(i => newArr.Add(i));
            attributes.Add(new TableAttribute("Permissioned", newArr.ToString()));
            this.add(DERIVATION_INFO_TABLE, "DocumentId", id.ToString(),
                attributes.ToArray());
        }

        private void setDocumentInfo(DocumentInfo info) {
            this.setDocumentInfo(info.Title, info.Owner, info.AnnotationCount,
                info.Id, info.Author, info.IsArchived, info.IsPublic,
                info.IsOpen, info.Permissioned);
        }

        public void RemoveDocumentPermission(Guid documentId, string user) {
            var docInfo = this.GetDocumentInfo(documentId);
            if (!docInfo.Permissioned.Contains(user)) {
                return;
            }
            docInfo.Permissioned.Remove(user);
            this.setDocumentInfo(docInfo);
        }

        public void AddDocumentPermission(Guid documentId, string user) {
            var docInfo = this.GetDocumentInfo(documentId);
            if (docInfo.Permissioned.Contains(user)) {
                return;
            }
            docInfo.Permissioned.Add(user);
            this.setDocumentInfo(docInfo);

            var matches = this.get(USER_SHARED_WITH_ME, "UserId", user);
            JArray shared = new JArray();
            if(matches.Count > 0) {
                shared = JArray.Parse(matches[0]["SharedWithMe"]);
            }
            shared.Add(documentId);
            this.add(USER_SHARED_WITH_ME, "UserId", user,
                new TableAttribute("SharedWithMe", shared.ToString()));
        }


        public void ModifyUserData() {
            var users = this.GetAllUsers(100);
            foreach (var user in users) {
                user.Created = DateTime.Now.Ticks;
                this.UpdateUser(user);
            }
        }

        public JObject RetrieveUser(string userId) {
            throw new Exception();
        }

        private List<Dictionary<string, string>> get(
            string table,
            string keyName,
            string keyValue) {
            string prefixedKeyName = ":v_" + keyName;
            AttributeValue av = new AttributeValue { S = keyValue };
            var response = this.client.Query(new QueryRequest(table) {
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>() {
                    { prefixedKeyName, av}
                },
                KeyConditionExpression = string.Format("{0} = {1}", keyName, prefixedKeyName),
            });
            if (response.Items.Count == 0) {
                return new List<Dictionary<string,string>>();
            }
            log.InfoFormat("Queried: {0}, {1}, {2}", table, keyName, keyValue);
            return response.Items.Select(i => i.ToDictionary(j => j.Key, j => j.Value.S)).ToList();
        }

        private const string USER_TABLE = @"derivation_users";
        private const string USER_DERIVATIONS = @"derivation_userDerivations";
        private const string DERIVATIONS_TABLE = @"derivation_derivations";
        private const string DERIVATION_INFO_TABLE = @"derivation_derivationInfo";
        private const string ANNOTATION_TABLE = @"annotation_annotations";
        private const string USER_ANNOTATIONS_TABLE = @"annotation_userAnnotations";
        private const string DOCUMENT_ANNOTATIONS_TABLE = @"annotation_documentAnnotations";
        private const string USER_SHARED_WITH_ME = @"annotation_userIdSharedWithMe";


        public Dictionary<string, string> GetUser(
            string userId
            ) {
                string indexName = "UserId";
                string propertyName = "UserId";
                var matches = this.get(USER_TABLE, propertyName, userId);
                log.InfoFormat("Queried {0}, {1}, {2}, {3}", USER_TABLE, indexName, propertyName, userId);
                var match = matches.SingleOrDefault();
                return match;
        }

        public void AddNewUser(string userId, JObject value) {
            this.add(USER_TABLE, "UserId", userId, new TableAttribute("UserInfo", value.ToString()));
        }

        private DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private enum ValueType { S, N };
        private struct TableAttribute {
            public TableAttribute(string name, string value, ValueType type = ValueType.S)
                : this() {
                this.AttributeName = name;
                this.AttributeValue = value;
                this.AttributeType = type;
            }
            public string AttributeValue { get; private set; }
            public string AttributeName { get; private set; }
            public ValueType AttributeType { get; set; }
            public AttributeValue ToAttributeValue() {
                switch (this.AttributeType) {
                    case ValueType.S:
                        return new AttributeValue { S = this.AttributeValue };
                    case ValueType.N:
                        return new AttributeValue { N = this.AttributeValue };
                    default:
                        throw new Exception("Unknown type");
                }
            }
        }

        private void add(string table, string keyName, string key, params TableAttribute[] values) {
            Dictionary<string, AttributeValue> attributes = new Dictionary<string, AttributeValue>();
            attributes[keyName] = new AttributeValue { S = key };
            foreach (var v in values) {
                attributes[v.AttributeName] = new AttributeValue { S = v.AttributeValue };
            }
            var response = client.BatchWriteItem(new BatchWriteItemRequest() {
                RequestItems = new Dictionary<string, List<WriteRequest>>() {
                    {
                      table,
                      new List<WriteRequest>() {
                           new WriteRequest(
                               new PutRequest(
                                   attributes
                                   )
                                )
                            }
                        }
                    },
            }
            );
            Debug.Assert(response.HttpStatusCode == System.Net.HttpStatusCode.OK);
            Debug.Assert(response.UnprocessedItems.Count == 0);
            log.Info(string.Format("Wrote Table: {0}, key: {1}, values: {2}", table, key, string.Join(",", values.Select(i => i.AttributeValue))));
        }


        private void delete(string table, string keyName, string keyValue) {
            this.client.DeleteItem(table,
                new Dictionary<string, AttributeValue>() {
                    {
                        keyName, new AttributeValue { S = keyValue }
                    }
                }
            );
            log.InfoFormat("Delete: {0}, {1}, {2}", table, keyValue, keyName);
        }

        private List<Dictionary<string, string>> scan(string table, int limit) {
            var response = this.client.Scan(new ScanRequest(table) {
                Limit = limit,
            });
            var toReturn = response.Items.Select(i => i.ToDictionary(j => j.Key, j => j.Value.S)).ToList();
            log.InfoFormat("Scan got {0} from: {1}", toReturn.Count, table);
            return toReturn;
        }

        public List<UserModel> GetAllUsers(int limit) {
            return this.scan(USER_TABLE, limit).Select(i => {
                var info = JObject.Parse(i["UserInfo"]);
                return UserModel.FromJson(info);
            }).ToList();
        }

        public bool UpdateUser(UserModel user) {
            try {
                var match = this.GetUser(user.UserId);
                user.Password = JObject.Parse(match["UserInfo"])["Password"].Value<string>();
                this.add(USER_TABLE, "UserId", user.UserId, new TableAttribute("UserInfo", user.ToJson().ToString()));
                return true;
            } catch (Exception ex) {
                log.Error("Update user failed", ex);
                return false;
            }
        }

        public DocumentInfo GetDocumentInfo(Guid id) {
            var doc = this.get(DERIVATION_INFO_TABLE, "DocumentId", id.ToString());
            return DocumentInfo.FromDictionary(doc[0]);
        }

        public IEnumerable<AnnotationDataModel> GetUserAnnotations(string userId) {
            var a = this.get(USER_ANNOTATIONS_TABLE, "UserId", userId);
            JArray ids;
            if (a.Count == 0) {
                ids = JArray.Parse("[]");
            } else {
                ids = JArray.Parse(a[0]["AnnotationIds"]);
            }
            foreach (var id in ids.Select(i => i.ToString())) {
                var asGuid = Guid.Parse(id);
                yield return this.GetAnnotation(asGuid);
            }
        }

        private IEnumerable<Guid> getUserDocumentIds(string userId) {
            var a = this.get(USER_DERIVATIONS, "UserId", userId);
            if (a.Count == 0) {
                yield break;
            }
            var ids = JArray.Parse(a[0]["DocumentIds"]);
            foreach (var id in ids) {
                Guid docId = Guid.Parse(id.Value<string>());
                yield return docId;
            }
        }

        private List<Guid> getDocumentsSharedWithMe(string userId) {
            List<Guid> toReturn = new List<Guid>();
            var sharedIds = this.get(USER_SHARED_WITH_ME, "UserId", userId);
            if (sharedIds.Count == 0) {
                return toReturn;
            }
            JArray arr = JArray.Parse(sharedIds.First()["SharedWithMe"]);
            foreach (var id in arr) {
                toReturn.Add(Guid.Parse(id.ToString()));
            }
            return toReturn;
        }

        public IEnumerable<DocumentInfo> GetUserDocuments(string userId) {
            List<DocumentInfo> toReturn = new List<DocumentInfo>();
            var sharedWithMe = this.getDocumentsSharedWithMe(userId);
            toReturn.AddRange(sharedWithMe.Select(i => this.GetDocumentInfo(i)));
            toReturn.AddRange(getUserDocumentIds(userId).Select(docId => {
                int count = this.GetAnnotationCount(docId);
                var r2 = this.GetDocumentInfo(docId);
                r2.AnnotationCount = count;
                return r2;
            }));
            return toReturn;
        }

        public void ArchiveAnnotation(Guid id) {
            var annotationDict = this.get(ANNOTATION_TABLE, "AnnotationId", id.ToString())[0];
            var ticks = long.Parse(annotationDict["Timestamp"]);
            string body = annotationDict["Body"];
            string author = annotationDict["Author"];
            this.AddAnnotation(id, ticks, body, author, isArchived:true);
        }

        public AnnotationDataModel GetAnnotation(Guid id) {
            var annotation = this.get(ANNOTATION_TABLE, "AnnotationId", id.ToString())[0];
            return AnnotationDataModel.FromDictionary(annotation);
        }

        public DocumentModel GetDocument(Guid id) {
            var doc = this.get(DERIVATIONS_TABLE, "DocumentId", id.ToString())[0];
            var toReturn = DocumentModel.FromDictionary(doc);
            var info = this.GetDocumentInfo(id);
            toReturn.Info = info;
            return toReturn;
        }

        public bool AddDocument(DocumentModel doc) {
            this.add(DERIVATIONS_TABLE, "DocumentId", doc.Id.ToString(),
                new TableAttribute("Body", doc.Body));
            this.setDocumentInfo(doc.Info);
            var ids = this.getUserDocumentIds(doc.Owner).ToList();
            if (ids.Any(i => i.ToString() == doc.Id.ToString())) {
                return true;
            }
            ids.Add(doc.Id);
            JArray a = new JArray();
            foreach (var id in ids) {
                a.Add(id);
            }
            this.add(USER_DERIVATIONS, "UserId", doc.Owner,
                new TableAttribute("DocumentIds", a.ToString()));
            return true;
        }

        public void AddAnnotation(Guid id, long ticks, string body, string author, bool isArchived = false) {
            this.add(ANNOTATION_TABLE, "AnnotationId", id.ToString(),
                new TableAttribute("Timestamp", ticks.ToString(), ValueType.N),
                new TableAttribute("Body", body),
                new TableAttribute("Author", author),
                new TableAttribute("IsArchived", isArchived.ToString()));
        }

        public void AddAnnotationAndLinkToUser(NewAnnotationModel newAnnotation, string userId) {
            this.AddAnnotation(newAnnotation.AnnotationId,
                DateTime.Now.Ticks,
                newAnnotation.Body, userId);
            var a = this.get(USER_ANNOTATIONS_TABLE, "UserId", userId);
            JArray ids;
            if (a.Count == 0) {
                ids = JArray.Parse("[]");
            } else {
                ids = JArray.Parse(a[0]["AnnotationIds"]);
            }
            ids.Add(newAnnotation.AnnotationId);
            this.add(USER_ANNOTATIONS_TABLE, "UserId", userId,
                new TableAttribute("AnnotationIds", ids.ToString()));

            string documentId = newAnnotation.DocumentId.ToString();
            a = this.get(DOCUMENT_ANNOTATIONS_TABLE, "DocumentId", documentId);
            if (a.Count == 0) {
                ids = JArray.Parse("[]");
            } else {
                ids = JArray.Parse(a[0]["AnnotationIds"]);
            }
            ids.Add(newAnnotation.AnnotationId);
            this.add(DOCUMENT_ANNOTATIONS_TABLE, "DocumentId", documentId,
                new TableAttribute("AnnotationIds", ids.ToString()));
        }

        public List<AnnotationModel> GetAnnotations(Guid documentId, DocumentModel doc) {
            //TODO: check the permissions on this user
            var a = this.get(DOCUMENT_ANNOTATIONS_TABLE, "DocumentId", documentId.ToString());
            if (a.Count == 0) {
                return new List<AnnotationModel>();
            }
            var ids = JArray.Parse(a[0]["AnnotationIds"]);
            var annotations = ids.Select(i => this.get(ANNOTATION_TABLE, "AnnotationId", i.ToString()));
            return annotations.Select(i => AnnotationModel.FromDictionary(i[0], doc)).ToList();
        }

        public int GetAnnotationCount(Guid documentId) {
            var a = this.get(DOCUMENT_ANNOTATIONS_TABLE, "DocumentId", documentId.ToString());
            if (a.Count == 0) {
                return 0;
            }
            var ids = JArray.Parse(a[0]["AnnotationIds"]);
            return ids.Count;
        }

        public void ArchiveDocument(Guid docId) {
            var doc = this.GetDocument(docId);
            doc.IsArchived = true;
            this.AddDocument(doc);
        }

        public void DeleteDocument(Guid docId, string userId) {
            var a = this.get(USER_DERIVATIONS, "UserId", userId);
            var ids = JArray.Parse(a[0]["DocumentIds"]);
            var matches = ids.Where(i => i.ToString() == docId.ToString()).ToList();
            foreach (var match in matches) {
                ids.Remove(match);
            }
            this.add(USER_DERIVATIONS, "UserId", userId,
                new TableAttribute("DocumentIds", ids.ToString()));
            this.delete(DERIVATIONS_TABLE, "DocumentId", docId.ToString());
        }

        public void UpdateAnnotation(UpdateAnnotationModel model) {
            var id = model.Id;
            var annotationDict = this.get(ANNOTATION_TABLE, "AnnotationId", id.ToString())[0];
            var ticks = long.Parse(annotationDict["Timestamp"]);
            string body = model.Body;
            string author = annotationDict["Author"];
            this.AddAnnotation(id, ticks, body, author);
        }


        public void UserDocumentPermissions(Guid id, string userId, out bool canView, out bool canAnnotate) {
            canView = false;
            canAnnotate = false;
            var docInfo = this.GetDocumentInfo(id);
            if (docInfo.Permissioned.Contains(userId)) {
                canView = true;
                canAnnotate = true;
                return;
            }
            if (docInfo.IsPublic) {
                canView = true;
                if (docInfo.IsOpen) {
                    canAnnotate = true;
                }
            }
        }
    }
}