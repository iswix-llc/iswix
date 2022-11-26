using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FireworksFramework.Managers;

namespace IsWiXAutomationInterface
{
    public static class IsWiXValidationHelper
    {
        public static bool IsValidIdentifier(string id)
        {
            bool valid = true;
            string validationrequest = id.ToLower();

            const string FIRST_CHARS = "_abcdefghijklmnopqrstuvwxyz";
            const string REMAINING_CHARS = "_.abcdefghijklmnopqrstuvwxyz0123456789";

            if(!FIRST_CHARS.Contains(validationrequest[0].ToString())||string.IsNullOrEmpty(validationrequest))
            {
                valid = false;
            }
            else
            {
                foreach (var letter in validationrequest)
	            {
		            if(!REMAINING_CHARS.Contains(letter))
                    {
                        valid = false;
                        break;
                    }
	            }
            }
            return valid;
        }
    }
}
