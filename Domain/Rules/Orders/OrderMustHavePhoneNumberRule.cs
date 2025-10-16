using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Rules.Orders
{
    public class OrderMustHavePhoneNumberRule : IBusinessRule
    {

        private readonly string _phoneNumber;

        public OrderMustHavePhoneNumberRule(string phoneNumber)
        {
            _phoneNumber = phoneNumber;
        }

        public bool IsBroken()
        {
       
            if (string.IsNullOrWhiteSpace(_phoneNumber))
                return true;

            var digitsOnly = new string(_phoneNumber.Where(char.IsDigit).ToArray());

            if (digitsOnly.Length <= 11)
                return true;
        
            //if (!IsValidPhoneNumberFormat(_phoneNumber))
            //    return true;

            return false;
        }

        public string Message => "Order must have a valid phone number with at least 10 digits.";

       
        //private bool IsValidPhoneNumberFormat(string phoneNumber)
        //{
          
        //    var usPattern = @"^\(?\d{3}\)?[-.\s]?\d{3}[-.\s]?\d{4}$";

           
        //    var internationalPattern = @"^[+]?[1-9][\d]{0,15}$";
  
        //    return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, usPattern) ||
        //           System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, internationalPattern);
        //}
    }
}
