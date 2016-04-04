using System;

namespace Derivation.Web.Models
{
    public sealed class UserIdDerivationId
    {
        public UserIdDerivationId(string userId, Guid? derivationId)
        {
            UserId = userId;
            DerivationId = derivationId;
        }

        public string UserId { get; set; }
        public Guid? DerivationId { get; set; }
    }
}