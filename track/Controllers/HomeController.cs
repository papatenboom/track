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

        public JsonResult GetDatasets(int id)
        {
            Dictionary<int, string> datasetDict = DatabaseManager.getDatasetLabels();
            return Json(JsonConvert.SerializeObject(datasetDict), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataset(int id)
        {
            Dataset dataset;
            dynamic datasetJObject = new JObject();

            try
            {
                dataset = DatabaseManager.getDataset(id);
                

                datasetJObject.label = dataset.Label;
                datasetJObject.ids = new JArray(dataset.getSeriesIds());
                datasetJObject.series = new JArray(dataset.getSeries());
                datasetJObject.types = new JArray(dataset.getSeriesTypes());
                datasetJObject.colors = new JArray(dataset.getSeriesColors());
                datasetJObject.notes = new JArray(dataset.getNotes());
                foreach (var s in dataset.getSeries())
                {
                    datasetJObject[s] = new JArray(dataset.getProperty(s));
                }
                datasetJObject.records = new JArray(dataset.getDateTimes());
                datasetJObject.span = dataset.getTimeSpan();
            
                // Set cookie
                HttpCookie lastId = new HttpCookie("lastDatasetId");
                lastId.Value = id.ToString();
                lastId.Expires = DateTime.Now.AddYears(10);

                Response.Cookies.Add(lastId);
                
            }
            catch (SqlException ex)
            {
                datasetJObject["error"] = ex.ToString();
            }

            return Json(JsonConvert.SerializeObject(datasetJObject), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDatasetSeries(int id)
        {
            Dataset cur = DatabaseManager.getDataset(id);

            return Json(JsonConvert.SerializeObject(cur.getSeries()), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult CreateDataset()
        {
            string datasetLabel = Request["datasetLabel"];

            List<string> labels = Request["labels"].Split(',').ToList<string>();
            List<string> typeStrings = Request["types"].Split(',').ToList<string>();
            List<int> typeIds = new List<int>();

            foreach (string type in typeStrings)
            {
                if (type.ToLower() == "integer")
                {
                    typeIds.Add(1);
                } else
                {
                    typeIds.Add(2);
                }
                Debug.WriteLine(type.ToString());
            }

            // TODO : replace user id 
            DatabaseManager.createDataset(1, datasetLabel, labels, typeIds);

            return Json("");
        }

        [HttpPost]
        public JsonResult SaveRecord()
        {
            // Store POST values
            int datasetId = Int32.Parse(Request["id"]);
            DateTime dateTime = DateTime.Parse(Request["datetime"]);

            List<string> labels = Request["labels"].Split(',').ToList<string>();
            List<string> values = Request["values"].Split(',').ToList<string>();

            string note = Request["note"];
            
            DatabaseManager.saveRecord(datasetId, labels, values, dateTime, note);

            return Json(true);
        }

        [HttpPost]
        public JsonResult UpdateDataset()
        {

            if (Request["datasetLabel"] != null)
            {
                Debug.WriteLine(Request["datasetId"]);
                Debug.WriteLine(Request["datasetLabel"]);

                int datasetId = Int32.Parse(Request["datasetId"]);
                string datasetLabel = Request["datasetLabel"];

                DatabaseManager.updateDataset(datasetId, datasetLabel);
            }

            if (Request["labels"] != null)
            {
                Debug.WriteLine(Request["id"]);
                Debug.WriteLine(Request["labels"]);
                Debug.WriteLine(Request["colors"]);

                List<string> ids = Request["ids"].Split(',').ToList<string>();
                List<string> labels = Request["labels"].Split(',').ToList<string>();
                List<string> colors = Request["colors"].Split(',').ToList<string>();

                for (int i = 0; i < ids.Count; i++)
                {
                    int seriesId = Int32.Parse(ids[i]);

                    DatabaseManager.updateSeries(seriesId, labels[i], colors[i]);
                }
            }

            return Json(true);
        }

        [HttpPost]
        public JsonResult UpdateSeries()
        {

            return Json(true);
        }
    }
}