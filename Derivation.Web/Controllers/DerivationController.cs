using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Derivation.Web.Models;
using Derivation.Web.Util;

namespace Derivation.Web.Controllers
{
    public class DerivationController : Controller
    {
        // GET: Derivation
        public ActionResult Index(Guid id, bool edit)
        {
            var user = IdentityUtil.GetCurrentUser();

            return View("Index", new DerivationIdUserId()
            {
                UserId = user.UserId,
                Role = user.Role,
                DerivationId = id
            });
        }
    }
}