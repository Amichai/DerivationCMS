using Derivation.Web.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Derivation.Web.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index(string userId) {
            return View("Index", model:userId);
        }
    }
}
