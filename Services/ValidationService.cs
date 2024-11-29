using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using INVApp.Interfaces;

namespace INVApp.Services
{
    public class ValidationService : IValidationService
    {
        public bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return false;

            // Regex explanation:
            // - (?=.*[a-z]): At least one lowercase letter
            // - (?=.*[A-Z]): At least one uppercase letter
            // - (?=.*\d): At least one digit
            // - (?=.*[@$!%*?&]): At least one special character N/A
            // - .{8,64}: Between 8 and 64 characters long
            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,64}$");
        }

        public bool ValidatePasscode(string passcode)
        {
            // Check if passcode is null or less than 4 characters
            if (string.IsNullOrEmpty(passcode) || passcode.Length < 4)
                return false;

            // Ensure the passcode contains only numeric characters
            return Regex.IsMatch(passcode, @"^\d+$");
        }

        public bool ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            // Allow only letters, spaces, and hyphens, limit length to 2-40 characters
            return Regex.IsMatch(name, @"^[a-zA-Z\s\-]{2,40}$");
        }

        public bool ValidatePhone(string phoneNumber)
        {
            // Allow digits, spaces, dashes, and parentheses, with 7-15 characters
            return Regex.IsMatch(phoneNumber, @"^\+?[0-9\s\-()]{7,15}$");
        }

        public bool ValidateAge(int age)
        {
            return age >= 0 && age <= 110; // Valid age range
        }

        public bool ValidateDate(DateTime date)
        {
            return date <= DateTime.Now; // Date should not be in the future
        }
    }
}
