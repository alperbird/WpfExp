using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.MainPage
{
    public class OpenFilePaths // bunu RelayCommand kullanacak şekilde ayarlayacağız.
    {
        public int FileId { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Customer { get; set; }
        public string Brand { get; set; }
        public string Series { get; set; }
    }
}
