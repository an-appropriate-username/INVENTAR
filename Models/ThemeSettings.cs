using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Models
{
	public class ThemeSettings
	{
		[PrimaryKey]
		public int Id { get; set; }
		public ThemeType Theme { get; set; }

		public ThemeSettings()
		{
			Id = 0;
			Theme = ThemeType.Default;
		}
	}

	public enum ThemeType
	{
		Light, // Light theme        
		Dark,  // Dark theme 
		Default, // Follows system settings 
		Blue // blue theme }
	}
}
