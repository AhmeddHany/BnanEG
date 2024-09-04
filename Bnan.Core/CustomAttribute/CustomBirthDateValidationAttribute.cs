using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bnan.Core.CustomAttribute
{
    public class CustomBirthDateValidationAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        private readonly DateTime _maxDate;

        public CustomBirthDateValidationAttribute(int minAge)
        {
            _minAge = minAge;
            _maxDate = DateTime.Today.AddYears(-minAge);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("requiredFiled");
            }

            var birthDate = (DateTime?)value;

            if (birthDate > _maxDate)
            {
                return new ValidationResult("requiredAgeAtLeast15");
            }

            return ValidationResult.Success;
        }
    }
}
