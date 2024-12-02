using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Models
{
    public class TaxSettings
    {
        [PrimaryKey]
        public int Id { get; set; }
        public double GST { get; set; }
        public bool Inclusive { get; set; }

        public TaxSettings()
        {
            Id = 0;
            GST = 15.0; // Set a default GST value 
            Inclusive = true; // Default to exclusive GST
        }

    }
}
