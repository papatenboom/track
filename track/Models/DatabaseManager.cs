using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;

namespace track.Models
{

    public static class DatabaseManager
    {

        //
        public static string connString = ConfigurationManager.ConnectionStrings["track"].ConnectionString;


        public static string testConnection()
        {
            try
            {

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    SqlCommand cmd;
                    SqlDataReader r;

                    conn.Open();

                    cmd = new SqlCommand("SELECT COUNT(*) FROM [User]", conn);
                    r = cmd.ExecuteReader();

                    while (r.Read())
                    {

                    }
                }

                return "200";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        // Add user
        public static int createUser(string username, string password)
        {
            int userId = 0, userCount;
            
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd;
                SqlDataReader r;

                conn.Open();

                cmd = new SqlCommand("SELECT COUNT(*) FROM [User] WHERE [Username]='" + username + "'", conn);
                userCount = (int)cmd.ExecuteScalar();

                // If username already exists
                if (userCount > 0)
                {
                    return 0;

                // Otherwise, create new user
                } else {

                    cmd = new SqlCommand("INSERT INTO [User] ([Username], [Password]) VALUES ('" + username + "', '" + CalculateMD5Hash(password) + "')", conn);
                    cmd.ExecuteNonQuery();

                    cmd = new SqlCommand("SELECT [Id] FROM [User] WHERE [Username]='" + username + "'", conn);
                    r = cmd.ExecuteReader();

                    while (r.Read())
                    {
                        userId = r.GetInt32(r.GetOrdinal("id"));
                    }
                }
                
                conn.Close();
            }

            return userId;
        }

        // Return dictionary <id, label> of all datasets
        public static Dictionary<int, string> getDatasetLabels()
        {
            Dictionary<int, string> datasetDict = new Dictionary<int, string>();
            
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd;
                SqlDataReader r;

                cmd = new SqlCommand("SELECT * FROM [Dataset]", conn);


                conn.Open();
                r = cmd.ExecuteReader();

                while (r.Read())
                {
                    datasetDict.Add(r.GetInt32(r.GetOrdinal("Id")), r.GetString(r.GetOrdinal("Label")));
                }
                conn.Close();
            }

            return datasetDict;
        }

        public static int createDataset(int userId, string label, List<string> seriesLabels, List<int> typeIds)
        {

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd;
                SqlDataReader r;

                conn.Open();

                // Create dataset
                cmd = new SqlCommand("CreateDataset", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@UserId", userId));
                cmd.Parameters.Add(new SqlParameter("@Label", label));
                cmd.Parameters.Add("@returnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                cmd.ExecuteNonQuery();

                int datasetId = (int)cmd.Parameters["@returnValue"].Value;

                for (int i = 0; i < seriesLabels.Count; i++)
                {

                    cmd = new SqlCommand("AddSeries", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.Add(new SqlParameter("@DatasetId", datasetId));
                    cmd.Parameters.Add(new SqlParameter("@TypeId", typeIds[i]));
                    cmd.Parameters.Add(new SqlParameter("@Label", seriesLabels[i]));
                    cmd.ExecuteNonQuery();
                }
                
            }

            return 0;
        }

        // Return dataset object by id
        public static Dataset getDataset(int datasetId)
        {
            Dataset dataset;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd;
                SqlDataReader r;

                string datasetLabel = "";

                // Get label of dataset with matching id
                cmd = new SqlCommand("SELECT * FROM [Dataset] WHERE [Id]=" + datasetId, conn);

                // TODO: Fix so detects if null or more than 1
                conn.Open();
                r = cmd.ExecuteReader();
                while (r.Read())
                {
                    datasetLabel = r.GetString(r.GetOrdinal("Label"));
                    
                }
                r.Close();


                // Get dataset series list
                cmd = new SqlCommand("GetSeries", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@DatasetId", datasetId));
                r = cmd.ExecuteReader();

                List<int> seriesIdList = new List<int>();
                List<string> seriesList = new List<string>();
                List<string> seriesTypeList = new List<string>();
                List<string> seriesColorList = new List<string>();
                while (r.Read())
                {
                    int seriesId = r.GetInt32(r.GetOrdinal("SeriesId"));
                    seriesIdList.Add(seriesId);

                    string seriesLabel = r.GetString(r.GetOrdinal("Label"));
                    seriesList.Add(seriesLabel);

                    string seriesType = r.GetString(r.GetOrdinal("SeriesType"));
                    seriesTypeList.Add(seriesType);

                    if (r.IsDBNull(r.GetOrdinal("Color")))
                    {
                        seriesColorList.Add(null);
                    } else
                    {
                        string seriesColor = r.GetString(r.GetOrdinal("Color"));
                        seriesColorList.Add(seriesColor);
                    }
                }
                r.Close();

                //
                dataset = new Dataset(datasetLabel, seriesIdList, seriesList, seriesTypeList, seriesColorList);


                // Get records with matching dataset id
                cmd = new SqlCommand("GetRecords", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@Id", datasetId));
                r = cmd.ExecuteReader();

                while (r.Read())
                {
                    Dictionary<string, object> props = new Dictionary<string, object>();

                    DateTime dt = r.GetDateTime(r.GetOrdinal("DateTime"));
                    string propString = r.GetString(r.GetOrdinal("Properties"));
                    string note = "";
                    if (!r.IsDBNull(r.GetOrdinal("Text")))
                        note = r.GetString(r.GetOrdinal("Text"));

                    var test = propString.Split(';');

                    foreach (var p in test)
                    {
                        int colon = p.IndexOf(':');
                        string key = p.Substring(1, colon - 1);
                        string val = p.Substring(colon + 1, p.Length - colon - 1);

                        if (props.ContainsKey(key))
                        {
                            props[key] = val;
                        } else
                        {
                            props.Add(key, val);
                        }
                    }

                    //Debug.WriteLine(dt.ToString() + " - " + props.ToString());

                    if (!string.IsNullOrEmpty(note))
                        dataset.createRecord(dt, props, note);
                    else
                        dataset.createRecord(dt, props);


                    //Debug.WriteLine(r.GetDateTime(r.GetOrdinal("DateTime")).ToString() + " - " + r.GetString(r.GetOrdinal("Properties")));
                }
                r.Close();

                /*
                cmd = new SqlCommand("SELECT [Series].[Id], [Label], [Name] AS 'Type' FROM [Series] JOIN [SeriesType] ON [TypeId]=[SeriesType].[Id] WHERE [DatasetId]=" + datasetId, conn);
                r = cmd.ExecuteReader();

                List<int> ids = new List<int>();
                List<string> labels = new List<string>();
                List<string> types = new List<string>();

                while (r.Read())
                {
                    ids.Add(r.GetInt32(r.GetOrdinal("Id")));
                    labels.Add(r.GetString(r.GetOrdinal("Label")));
                    types.Add(r.GetString(r.GetOrdinal("Type")));
                }
                r.Close();

                foreach (int id in ids)
                {
                    cmd = new SqlCommand("SELECT [DateTime], [Value] FROM [Record] JOIN [Property] ON [Record].[Id] = [Property].[RecordId] WHERE [SeriesId]=" + id, conn);
                    r = cmd.ExecuteReader();

                    while (r.Read())
                    {

                    }

                }
                */

                conn.Close();
            }

            return dataset;
        }

        public static void saveRecord(int datasetId, List<string> labels, List<string> values, DateTime datetime, string note = "")
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd;
                SqlDataReader r;

                conn.Open();

                // Create record entry
                cmd = new SqlCommand("CreateRecord", conn) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add(new SqlParameter("@DatasetId", datasetId));
                cmd.Parameters.Add(new SqlParameter("@DateTime", datetime.ToString()));
                if (!string.IsNullOrEmpty(note))
                    cmd.Parameters.Add(new SqlParameter("@Note", note));
                cmd.Parameters.Add("@returnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                cmd.ExecuteNonQuery();

                int recordId = (int)cmd.Parameters["@returnValue"].Value;

                // Add properties
                for (var i = 0; i < labels.Count; i++)
                {
                    cmd = new SqlCommand("AddProperty", conn) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.Add(new SqlParameter("@RecordId", recordId));
                    cmd.Parameters.Add(new SqlParameter("@Label", labels[i]));
                    cmd.Parameters.Add(new SqlParameter("@Value", values[i]));

                    cmd.ExecuteNonQuery();
                }

            }

        }

        public static void updateDataset(int datasetId, string label)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("UpdateDataset", conn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@DatasetId", datasetId));
                    cmd.Parameters.Add(new SqlParameter("@Label", label));

                    cmd.ExecuteNonQuery();
                }

            }
        }

        public static void updateSeries(int seriesId, string label = null, string color = null)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("UpdateSeries", conn) { CommandType = CommandType.StoredProcedure })
                {
                    cmd.Parameters.Add(new SqlParameter("@SeriesId", seriesId));
                    if (label != null)
                        cmd.Parameters.Add(new SqlParameter("@Label", label));
                    if (color != null)
                        cmd.Parameters.Add(new SqlParameter("@Color", color));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Utility ------------------------------------------------------------------------------

        private static string CalculateMD5Hash(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                return sb.ToString();
            }
        }
    }
}