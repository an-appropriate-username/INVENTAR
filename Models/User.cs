using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace INVApp.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Unique 5-digit user identifier
        public int UserId { get; set; }
        public UserPrivilege Privilege { get; set; }

        // Enum for Privilege level
        public enum UserPrivilege
        {
            Basic = 1,
            Manager = 2,
            Admin = 3
        }


        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }


        public string? Password { get; set; }
        public string? Passcode { get; set; }


        public int ItemsScanned { get; set; }
        public int CustomersAdded { get; set; }
        public int TransactionsClosed { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime LastLogin { get; set; }

		public Theme UserTheme { get; set; }

		/// <summary>
		/// Checks if the user has the required privilege level or higher.
		/// </summary>
		/// <param name="requiredPrivilegeLevel">The minimum privilege level required.</param>
		/// <returns>True if the user's privilege level meets or exceeds the required level, false otherwise.</returns>
		public bool HasPrivileges(UserPrivilege requiredPrivilegeLevel)
        {
            return Privilege >= requiredPrivilegeLevel;
        }

        /// <summary>
        /// Returns a string representation of the user.
        /// </summary>
        /// <returns>A string showing the user's first name, ID, and privilege level.</returns>
        public override string ToString()
        {
            return $"{FirstName} (ID: {UserId}, Privilege: {Privilege})";
        }

        
    }

	public enum Theme
	{
		Default = 1,
		Blue = 2,
		Light = 3,
		Dark = 4
	}
}
