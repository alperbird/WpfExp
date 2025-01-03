using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.Models
{
    public class ProfileExm
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string filetype { get; set; }
        public byte[] filecontent { get; set; }
    }
}
