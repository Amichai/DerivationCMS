
using Newtonsoft.Json.Linq;

namespace Derivation.Web.Models
{
    public sealed class UserIdDerivation
    {
        public UserIdDerivation(string userId, JObject derivation = null)
        {
            UserId = userId;
            Derivation = derivation;
        }

        public string UserId { get; set; }
        public JObject Derivation { get; set; }
    }
}