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
using Newtonsoft.Json;

namespace track.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            /*
            List<string> returnValues = new List<string>() { "Can you see me" };

            string connString = ConfigurationManager.ConnectionStrings["track-remote"].ConnectionString;
            string sql = "SELECT * FROM [Dataset]";

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader r = cmd.ExecuteReader())
                        {

                            while (r.Read())
                            {
                                returnValues.Add(r.GetString(r.GetOrdinal("Label")));
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.ToString());
                returnValues.Add(ex.ToString());
            }*/


            return View();
        }

        public ActionResult Chart()
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
            string path = Server.MapPath("~/data.json");

            // Pass true to commit to database
            JSONAdapter adapter = new JSONAdapter(path, true);
            
            // Return # of datasets
            return Json(adapter.getDatasets().Count, JsonRequestBehavior.AllowGet);
        }

        // Make sure database connectivity exists
        public JsonResult TestDatabase()
        {
            string returnString;
            try
            {
                returnString = DatabaseManager.testConnection();
            } catch (Exception ex)
            {
                returnString = ex.ToString();
            }
            return Json(returnString, JsonRequestBehavior.AllowGet);
        }

        // Makes sure call to controller is functional
        public JsonResult Test()
        {
            return Json("controller works", JsonRequestBehavior.AllowGet);
        }
    }
}
 