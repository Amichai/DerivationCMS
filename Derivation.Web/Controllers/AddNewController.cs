using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Mvc;
using Amazon.S3.Util;
using Derivation.Web.Data;
using Derivation.Web.Models;
using Derivation.Web.Util;

namespace Derivation.Web.Controllers
{
    public class AddNewController : Controller
    {
        // GET: AddNew
        public ActionResult Index(Guid? id, bool edit = false)
        {
            var user = IdentityUtil.GetCurrentUser();
            return View("Index", model: new UserIdDerivationId(user.UserId, id));
        }
    }
}