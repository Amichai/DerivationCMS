using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Derivation.Web.Models
{
    public class DerivationIdUserId
    {
        public Guid DerivationId { get; set; }
        public string UserId { get; set; }
        public string Role { get; set; }
    }
}