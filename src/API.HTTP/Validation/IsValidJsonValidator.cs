using System;
using System.Text;
using FluentValidation.Validators;
using Newtonsoft.Json.Linq;

namespace ForgetMeNot.API.HTTP.Validation
{
	public class IsValidJsonValidator : PropertyValidator
	{
		public IsValidJsonValidator ()
			: base ("Property '{PropertyName}' is not valid JSON!")
		{
			//empty
		}

		protected override bool IsValid (PropertyValidatorContext context)
		{
			//hmm - using exceptions to enforce validation. but it is the only way i can think to do this without writing my own parser
			try {
				var jsonString = Encoding.UTF8.GetString((context.PropertyValue as byte[]));
				JContainer.Parse (jsonString);
				return true;
			} catch (Exception) {
				return false;
			}
		}
	}
}

