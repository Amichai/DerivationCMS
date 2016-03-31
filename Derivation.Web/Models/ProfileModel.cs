using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Derivation.Web.Models {
    public class ProfileModel {
        public string UserId { get; set; }
        public DateTime Created { get; set; }
        public string CreatedString { get; set; }
        public List<DerivationModel> Derivations { get; set; }
    }
}