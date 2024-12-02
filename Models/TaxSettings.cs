using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Models
{
    public class TaxSettings
    {

        public int Id { get; set; }
        public double GST { get; set; }

        public TaxSettings()
        {
            GST = 15.0; // Set a default GST value 
        }

    }
}
