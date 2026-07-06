using System;
using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    public class OptionalEmailAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return true;

            var email = value.ToString();

            if (string.IsNullOrWhiteSpace(email))
                return true;

            return new EmailAddressAttribute().IsValid(email);
        }
    }
}
