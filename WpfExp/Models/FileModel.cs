using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfExp.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string FileType { get; set; }
        public byte[] ContentSize { get; set; }
        public long FileSize { get; set; }

    }
}
