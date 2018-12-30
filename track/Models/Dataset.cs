using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace track.Models
{

    public class Dataset
    {
        public string Id { get; set; }
        public string Label { get; set; }

        private List<Record> RecordList = new List<Record>();
        private List<string> SeriesList = new List<string>();

        private DateTime startDateTime { get; set; }
        private DateTime endDateTime { get; set; }

        
        public Dataset(string label)
        {
            Label = label;
        }

        public void createRecord(DateTime dateTime, Dictionary<string, object> props)
        {
            // Init start & end
            if (startDateTime.CompareTo(new DateTime()) == 0)
            {
                startDateTime = dateTime;
                endDateTime = dateTime;
            }
            
            // Test for new start or end
            if (dateTime.CompareTo(startDateTime) < 0)
                startDateTime = dateTime;
            if (dateTime.CompareTo(endDateTime) > 0)
                endDateTime = dateTime;

            //
            Record temp = new Record(dateTime);
            foreach (var p in props)
            {
                // Test for new series
                if (!SeriesList.Contains(p.Key))
                    SeriesList.Add(p.Key);

                // Assign property
                temp[p.Key] = p.Value;
            }
            RecordList.Add(temp);
        }

        public int recordCount()
        {
            return RecordList.Count;
        }

        public List<string> getSeries()
        {
            return SeriesList;
        }

       /* public Dictionary<string, double> getRecordDictionary()
        {
            Dictionary<string, double> nodeDict = new Dictionary<string, double>();

            foreach (Record n in RecordList)
            {
                //nodeDict.Add(n.DateTime.ToString(), n.Value);
            }

            return nodeDict;
        }*/
        
        public List<DateTime> getDateTimes()
        {
            List<DateTime> dtList = new List<DateTime>();

            foreach (Record n in RecordList)
                dtList.Add(n.DateTime);

            return dtList;
        }

        public TimeSpan getTimeSpan()
        {
            return new TimeSpan(endDateTime.Ticks - startDateTime.Ticks);
        }

        public List<Object> getProperty(string prop)
        {
            List<Object>pList = new List<Object>();

            foreach (Record r in RecordList)
            {
                pList.Add(r[prop]);
            }

            return pList;
        }
    }
}