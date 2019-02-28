using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace track.Models
{
    class Record
    {
        public DateTime DateTime { get; set; }

        private Dictionary<string, object> Properties = new Dictionary<string, object>();

        public string Note { get; set; }


        public Record(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public Record(DateTime dateTime, string note)
        {
            DateTime = dateTime;
            Note = note;
        }

        public Record(DateTime dateTime, Dictionary<string, object> props)
        {
            DateTime = dateTime;

            foreach (var prop in props)
            {
                Properties[prop.Key] = prop.Value;
            }
        }

        public Record(DateTime dateTime, Dictionary<string, object> props, string note)
        {
            DateTime = dateTime;

            foreach (var prop in props)
            {
                Properties[prop.Key] = prop.Value;
            }

            Note = note;
        }

        public object this[string name]
        {
            get {
                if (Properties.ContainsKey(name))
                {
                    return Properties[name];
                }
                return null;
            }
            set
            {
                Properties[name] = value;
            }
        }

    }
}
