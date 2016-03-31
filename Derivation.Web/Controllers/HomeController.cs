using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Derivation.Web.Controllers.api;
using Derivation.Web.Data;
using Derivation.Web.Util;

namespace Derivation.Web.Controllers {
    public class HomeController : Controller {
        public ActionResult Index(string userId = null) {
            ViewBag.Title = "Home Page";
            var sessionId = Request.Cookies["session_id"];
            bool isSessionValid = AuthenticationController.IsSessionIdValid(Request);
            if (!isSessionValid)
            {
                userId = IdentityUtil.ANONYMOUS_USERID;
                ViewBag.UserId = userId;
            }
            return View();
        }
    }
}
