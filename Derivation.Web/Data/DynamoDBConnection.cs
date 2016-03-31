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

namespace Derivation.Web.Data
{
    public class DynamoDBConnection
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IAmazonDynamoDB client;

        private DynamoDBConnection(RegionEndpoint endpoint = null) {
            if (endpoint == null) {
                endpoint = RegionEndpoint.USEast1;
            }
            client = new AmazonDynamoDBClient(endpoint);
        }

        public static DynamoDBConnection Instance = new DynamoDBConnection();

        private void setDerivationInfo(string title, string owner, Guid id, bool isArchived) {
            var attributes = new List<TableAttribute>();
            attributes.Add(new TableAttribute("Title", title));
            attributes.Add(new TableAttribute("Owner", owner));
            attributes.Add(new TableAttribute("Id", id.ToString()));
            attributes.Add(new TableAttribute("IsArchived", isArchived.ToString()));
            this.add(DERIVATION_INFO_TABLE, "DerivationId", id.ToString(),
                attributes.ToArray());
        }

        public void ModifyUserData()
        {
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
            var av = new AttributeValue { S = keyValue };
            var response = this.client.Query(new QueryRequest(table) {
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>() {
                    { prefixedKeyName, av}
                },
                KeyConditionExpression = string.Format("{0} = {1}", keyName, prefixedKeyName),
            });
            if (response.Items.Count == 0) {
                return new List<Dictionary<string, string>>();
            }
            log.InfoFormat("Queried: {0}, {1}, {2}", table, keyName, keyValue);
            return response.Items.Select(i => i.ToDictionary(j => j.Key, j => j.Value.S)).ToList();
        }

        private const string USER_TABLE = @"derivation_users";
        private const string USER_DERIVATIONS = @"derivation_userDerivations";
        private const string DERIVATIONS_TABLE = @"derivation_derivations";
        private const string DERIVATION_INFO_TABLE = @"derivation_derivationInfo";

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

        public enum ValueType { S, N };
        public struct TableAttribute {
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

        private void add(string table, string keyName, string key, params TableAttribute[] values)
        {
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


        private void delete(string table, string keyName, string keyValue)
        {
            this.client.DeleteItem(table,
                new Dictionary<string, AttributeValue>() {
                    {
                        keyName, new AttributeValue { S = keyValue }
                    }
                }
            );
            log.InfoFormat("Delete: {0}, {1}, {2}", table, keyValue, keyName);
        }

        private List<Dictionary<string, string>> scan(string table, int limit)
        {
           var response = this.client.Scan(new ScanRequest(table) {
                Limit = limit,
            });
            var toReturn = response.Items.Select(i => i.ToDictionary(j => j.Key, j => j.Value.S)).ToList();
            log.InfoFormat("Scan got {0} from: {1}", toReturn.Count, table);
            return toReturn;
        }

        public List<UserModel> GetAllUsers(int limit)
        {
            return this.scan(USER_TABLE, limit).Select(i => {
                var info = JObject.Parse(i["UserInfo"]);
                return UserModel.FromJson(info);
            }).ToList();
        }

        public bool UpdateUser(UserModel user)
        {
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

        private IEnumerable<Guid> getUserDerivationIds(string userId)
        {
            var a = this.get(USER_DERIVATIONS, "UserId", userId);
            if (a.Count == 0) {
                yield break;
            }
            var ids = JArray.Parse(a[0]["DerivationIds"]);
            foreach (var id in ids) {
                Guid docId = Guid.Parse(id.Value<string>());
                yield return docId;
            }
        }

        public IEnumerable<DerivationModel> GetAllDerivations(int limit)
        {
            return this.scan(DERIVATIONS_TABLE, limit).Select(i => DerivationModel.FromDictionary(i)).ToList();
        }

        public IEnumerable<DerivationModel> GetUserDerivations(string userId)
        {
            return getUserDerivationIds(userId).Select(docId => GetDerivation(docId));
        }

        public DerivationModel GetDerivation(Guid id) {
            var doc = this.get(DERIVATIONS_TABLE, "DerivationId", id.ToString())[0];
            var toReturn = DerivationModel.FromDictionary(doc);
            return toReturn;
        }

        public bool AddDerivation(DerivationModel derivation)
        {
            this.add(DERIVATIONS_TABLE, "DerivationId", derivation.Id.ToString(),
                derivation.GetTableAttributes());

            var ids = this.getUserDerivationIds(derivation.Owner).ToList();
            if (ids.Any(i => i.ToString() == derivation.Id.ToString())) {
                return true;
            }
            ids.Add(derivation.Id);
            JArray a = new JArray();
            foreach (var id in ids) {
                a.Add(id);
            }
            this.add(USER_DERIVATIONS, "UserId", derivation.Owner,
                new TableAttribute("DerivationIds", a.ToString()));
            return true;
        }

        public void ArchiveDocument(Guid docId) {
            var doc = this.GetDerivation(docId);
            doc.IsArchived = true;
            this.AddDerivation(doc);
        }

        public void DeleteDocument(Guid docId, string userId) {
            var a = this.get(USER_DERIVATIONS, "UserId", userId);
            var ids = JArray.Parse(a[0]["DerivationIds"]);
            var matches = ids.Where(i => i.ToString() == docId.ToString()).ToList();
            foreach (var match in matches) {
                ids.Remove(match);
            }
            this.add(USER_DERIVATIONS, "UserId", userId,
                new TableAttribute("DerivationIds", ids.ToString()));
            this.delete(DERIVATIONS_TABLE, "DerivationId", docId.ToString());
        }
    }
}