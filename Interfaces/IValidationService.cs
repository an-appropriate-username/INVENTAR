using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INVApp.Interfaces
{
    public interface IValidationService
    {
        bool ValidateEmail(string email);
        bool ValidatePassword(string password);

        bool ValidatePasscode(string passcode);
        bool ValidateName(string name);
        bool ValidatePhone(string phoneNumber);
        bool ValidateAge(int age);
        bool ValidateDate(DateTime date);

    }
}
