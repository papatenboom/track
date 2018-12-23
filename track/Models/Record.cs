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
        public double Value { get; set; }

        public string Note { get; set; }

        public Record(DateTime dateTime, double value)
        {
            DateTime = dateTime;
            Value = value;
        }

        public Record(DateTime dateTime, double value, string note)
        {
            DateTime = dateTime;
            Value = value;
            Note = note;
        }

    }
}
