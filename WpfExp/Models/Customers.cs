using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.Models
{
    public class Customers
    {
        public int CustomerID { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        // FK olanlar:
        public int CitiesID { get; set; }
        public int MachineID { get; set; }
        public int PBrandID { get; set; }
        public int PSeriesID { get; set; }
        public int PExmID { get; set; }

    }
}
