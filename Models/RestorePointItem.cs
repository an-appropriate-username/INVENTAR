using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Models
{
    public class RestorePointItem
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
