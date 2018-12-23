using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace track.Models
{
    public class JSONAdapter
    {
        private string dataJSON;

        private JObject dataJObject;

        private List<Dataset> dtList = new List<Dataset>();

        // Constructor - Parses JSON & creates List of Datasets
        public JSONAdapter(string jsonPath, bool toDatabase = false)
        {
            // Complete JSON string
            dataJSON = File.ReadAllText(jsonPath);

            dataJObject = JObject.Parse(dataJSON);

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["track"].ConnectionString))
            {
                SqlCommand cmd;
                SqlDataReader r;

                foreach (JProperty child in dataJObject.Properties())
                {
                    // Get list of datetime string
                    JArray dates = (JArray)child.Value.SelectToken("labels");
                    List<string> dateList = dates.ToObject<List<string>>();

                    // Get "datasets"
                    JObject datasets = (JObject)child.Value.SelectToken("datasets").First;


                    // Create new Dataset
                    Dataset testDataset = new Dataset((string)datasets.SelectToken("label"));

                    if (toDatabase)
                    {
                        cmd = new SqlCommand("INSERT INTO [dbo].[Dataset] ([UserId], [Label]) VALUES ('1', '" + testDataset.Label + "')", conn);

                        conn.Open();
                        r = cmd.ExecuteReader();
                        conn.Close();

                    }


                    Debug.WriteLine(testDataset.Label + " - " + toDatabase.ToString());
                    cmd = new SqlCommand("SELECT [Id] FROM [dbo].[Dataset] WHERE [Label]='" + testDataset.Label + "'", conn);
                    conn.Open();
                    r = cmd.ExecuteReader();

                    int id = 0;
                    while (r.Read())
                    {
                        id = r.GetInt32(r.GetOrdinal("Id"));
                    }
                    conn.Close();


                    // Get list of value from dataset
                    JArray values = (JArray)datasets.SelectToken("data");
                    List<string> valueList = values.ToObject<List<string>>();


                    // Create list of nodes in dataset
                    for (int i = 0; i < dateList.Count; i++)
                    {
                        DateTime dt = new DateTime();
                        try
                        {
                            dt = DateTime.Parse(dateList[i]);
                        }
                        catch (FormatException e)
                        {
                            Debug.WriteLine(e.ToString());
                        }

                        double val = 0;
                        try
                        {
                            val = Double.Parse(valueList[i]);
                        }
                        catch (FormatException e)
                        {
                            Debug.WriteLine(e.ToString());
                        }

                        // Add to object
                        testDataset.createRecord(dt, val);

                        // Add to database
                        if (toDatabase)
                        {
                            Debug.WriteLine(val);
                            cmd = new SqlCommand("INSERT INTO [dbo].[Record] ([DatasetId], [DateTime], [Value]) VALUES (" + id + ", '" + dt.ToString() + "', " + val + ")", conn);

                            conn.Open();
                            r = cmd.ExecuteReader();
                            conn.Close();

                        }

                    }

                    dtList.Add(testDataset);

                }
            }
        }

        // Returns a List of Dataset Label strings
        public List<string> getDatasetLabels()
        {
            List<string> labelList = new List<string>();

            foreach (Dataset dt in dtList)
            {
                labelList.Add(dt.Label);
            }

            return labelList;
        }

        // Returns List of Datasets
        public List<Dataset> getDatasets()
        {
            return dtList;
        }
        
    }
}