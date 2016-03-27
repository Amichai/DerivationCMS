using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Derivation.Web.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            var cookie = new HttpCookie("session_id");
            cookie.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Add(cookie);
            return RedirectToAction("Index", "Register");
        }

        public ActionResult Logout() {
            return RedirectToAction("Index", "Register");
        }
    }
}
