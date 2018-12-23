using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using track.Models;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Text;
using System.Security.Cryptography;

namespace track.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult CreateUser()
        {
            // Return new user id or 0
            return Json(DatabaseManager.createUser("admin", "asdf"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult CopyToDatabase()
        {
            // data.json file
            string path = Server.MapPath("~/data_120818.json");

            // Pass true to commit to database
            JSONAdapter adapter = new JSONAdapter(path, true);
            
            // Return # of datasets
            return Json(adapter.getDatasets().Count, JsonRequestBehavior.AllowGet);
        }
    }
}