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
        private string connString = ConfigurationManager.ConnectionStrings["track"].ConnectionString;

        private string dataJSON;

        private JObject dataJObject;

        private List<Dataset> dtList = new List<Dataset>();

        // Constructor - Parses JSON & creates List of Datasets
        public JSONAdapter(string jsonPath, bool toDatabase = false)
        {
            // Complete JSON string
            dataJSON = File.ReadAllText(jsonPath);

            dataJObject = JObject.Parse(dataJSON);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd;
                SqlDataReader r;
                conn.Open();

                foreach (JProperty child in dataJObject.Properties())
                {
                    // Get "datasets"
                    JObject datasets = (JObject)child.Value.SelectToken("datasets").First;

                    string label = (string)datasets.SelectToken("label");

                    // Create new Dataset
                    //Dataset testDataset = new Dataset(label);

                    int datasetId = 0, seriesId = 0, recordId = 0;
                    if (toDatabase)
                    {
                        cmd = new SqlCommand("INSERT INTO [dbo].[Dataset] ([UserId], [Label], [Archived]) VALUES ('1', '" + label + "', 0)", conn);
                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("SELECT [Id] FROM [dbo].[Dataset] WHERE [Label]='" + label + "'", conn);
                        r = cmd.ExecuteReader();

                        while (r.Read())
                        {
                            datasetId = r.GetInt32(r.GetOrdinal("Id"));
                        }
                        r.Close();

                    }

                    // Get list of value from dataset
                    JArray values = (JArray)datasets.SelectToken("data");
                    List<string> valueList = values.ToObject<List<string>>();

                    if (toDatabase)
                    {
                        // Need to determine type of data
                        bool intFlag = true;
                        foreach (var val in valueList)
                        {
                            if (!int.TryParse(val, out int result))
                            {
                                intFlag = false;
                            }
                        }

                        // Create new Series for Dataset
                        cmd = new SqlCommand("INSERT INTO [Series] ([DatasetId], [TypeId], [Label]) VALUES (" + datasetId + ", " + (intFlag ? 1 : 2) + ", 'Value')", conn);
                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("SELECT [Id] FROM [Series] WHERE [DatasetId]=" + datasetId, conn);
                        r = cmd.ExecuteReader();

                        while (r.Read())
                        {
                            seriesId = r.GetInt32(r.GetOrdinal("Id"));
                        }
                        r.Close();
                    }


                    // Get list of datetime string
                    JArray dates = (JArray)child.Value.SelectToken("labels");
                    List<string> dateList = dates.ToObject<List<string>>();

                    // Create list of nodes in dataset
                    for (int i = 0; i < dateList.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(dateList[i]))
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
                            //testDataset.createRecord(dt, val);

                            // Add to database
                            if (toDatabase)
                            {
                                Debug.WriteLine(dt.ToString());
                                cmd = new SqlCommand("INSERT INTO [Record] ([DatasetId], [DateTime]) VALUES (" + datasetId + ", '" + dt.ToString() + "')", conn);
                                cmd.ExecuteNonQuery();

                                cmd = new SqlCommand("SELECT [Id] FROM [Record] WHERE [DatasetId]=" + datasetId + " AND [DateTime]='" + dt.ToString() + "'", conn);
                                r = cmd.ExecuteReader();

                                while (r.Read())
                                {
                                    recordId = r.GetInt32(r.GetOrdinal("Id"));
                                }
                                r.Close();

                                cmd = new SqlCommand("INSERT INTO [Property] ([RecordId], [SeriesId], [Value]) VALUES (" + recordId + ", " + seriesId + ", " + val + ")", conn);
                                cmd.ExecuteNonQuery();
                            }
                        }

                    }

                    //dtList.Add(testDataset);

                }

                conn.Close();
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