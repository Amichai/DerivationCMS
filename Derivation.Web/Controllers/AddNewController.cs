using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Derivation.Web.Util;

namespace Derivation.Web.Controllers
{
    public class AddNewController : Controller
    {
        // GET: AddNew
        public ActionResult Index()
        {
            var user = IdentityUtil.GetCurrentUser();

            return View("Index", model: user.UserId);
        }
    }
}