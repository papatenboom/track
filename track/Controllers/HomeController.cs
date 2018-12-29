using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using track.Models;

namespace track.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Dictionary<int, string> datasetDict = DatabaseManager.getDatasetLabels();
            
            // Cookie for last viewed dataset id
            HttpCookie lastId = Request.Cookies["lastDatasetId"];

            if (lastId != null)
            {
                ViewData["lastId"] = lastId.Value;
            }

            // Returns List of Label strings
            return View(datasetDict);
        }

        public JsonResult GetDataset(int id)
        {

            Dataset cur = DatabaseManager.getDataset(id);

            dynamic records = new JObject();
            records.labels = new JArray(cur.getDateTimes());
            records.values = new JArray(cur.getProperty("Value"));
            records.span = cur.getTimeSpan();
            

            // Set cookie
            HttpCookie lastId = new HttpCookie("lastDatasetId");
            lastId.Value = id.ToString();
            lastId.Expires = DateTime.Now.AddYears(10);

            Response.Cookies.Add(lastId);

            return Json(JsonConvert.SerializeObject(records), JsonRequestBehavior.AllowGet);
        }
    }
}