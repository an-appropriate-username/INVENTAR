using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Models
{
    public class AudioSettings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public double Volume { get; set; }
        public bool IsEnabled { get; set; }
    }
}
