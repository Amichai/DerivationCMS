﻿using System;
using System.Collections.Generic;
using Derivation.Web.Data;
using Newtonsoft.Json.Linq;

namespace Derivation.Web.Models
{
    public sealed class DerivationModel
    {
        public List<DerivationStep> Steps = new List<DerivationStep>();

        public string Title { get; set; }

        public string Owner { get; set; }

        public Guid Id { get; set; }

        public bool IsArchived { get; set; }

        public string Description { get; set; }

        internal static DerivationModel FromDictionary(Dictionary<string, string> dict)
        {
            return new DerivationModel()
            {
                Title = dict["Title"],
                Owner = dict["Owner"],
                Id = Guid.Parse(dict["DerivationId"]),
                IsArchived = bool.Parse(dict["IsArchived"]),
                Description = dict["Description"],
            };
        }

        internal DynamoDBConnection.TableAttribute[] GetTableAttributes()
        {
            var arr = new JArray();
            foreach (var step in Steps)
            {
                arr.Add(step.ToJson());
            }

            return new[]
            {
                new DynamoDBConnection.TableAttribute("Title", Title),
                new DynamoDBConnection.TableAttribute("Owner", Owner),
                new DynamoDBConnection.TableAttribute("IsArchived", IsArchived.ToString()),
                new DynamoDBConnection.TableAttribute("Description", Description),
                new DynamoDBConnection.TableAttribute("Steps", arr.ToString()),
            };
        }
    }

    public class DerivationStep
    {
        public string Equation { get; set; }

        public string Transition { get; set; }

        public string Notes { get; set; }

        public JObject ToJson()
        {
            JObject toReturn = new JObject();
            toReturn["Equation"] = Equation;
            toReturn["Transition"] = Transition;
            toReturn["Notes"] = Notes;
            return toReturn;
        }
    }
}