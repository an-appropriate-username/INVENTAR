using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Models
{
    public class ToDoItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string? Task { get; set; }
        public bool? IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
