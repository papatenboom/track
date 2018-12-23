using System;
using System.Collections.Generic;
using System.Configuration;
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

        // Return dataset object by id
        public static Dataset getDataset(int id)
        {
            Dataset dataset = new Dataset("");

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd;
                SqlDataReader r;

                string label;

                // Get label of dataset with matching id
                cmd = new SqlCommand("SELECT * FROM [Dataset] WHERE [Id]=" + id, conn);

                // TODO: Fix so detects if null or more than 1
                conn.Open();
                r = cmd.ExecuteReader();
                while (r.Read())
                {
                    label = r.GetString(r.GetOrdinal("Label"));
                }
                conn.Close();


                // Get records with matching dataset id
                cmd = new SqlCommand("SELECT * FROM [Record] WHERE [SeriesId]=" + id, conn);

                conn.Open();
                r = cmd.ExecuteReader();
                
                while (r.Read())
                {
                    DateTime dt = r.GetDateTime(r.GetOrdinal("DateTime"));
                    Double val = Double.Parse(r.GetString(r.GetOrdinal("Value")));

                    dataset.createRecord(dt, val);
                }
                conn.Close();

            }

            return dataset;
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