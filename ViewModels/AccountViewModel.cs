using INVApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;   

        public AccountViewModel(DatabaseService databaseService) 
        { 
            _databaseService = databaseService;
        }  
    }
}
