using System.Text;
using FluentValidation;

namespace ForgetMeNot.API.HTTP.Validation
{
	public static class Extensions
	{
		public static IRuleBuilderOptions<T, TElement> IsValidJson<T, TElement>(this IRuleBuilder<T, TElement> ruleBuilder)
		{
			return ruleBuilder.SetValidator (new IsValidJsonValidator());
		}

		public static string AsString(this byte[] byteArray)
		{
			return Encoding.UTF8.GetString (byteArray);
		}

		public static byte[] AsUtf8EncodedByteArray(this string source)
		{
			return Encoding.UTF8.GetBytes (source);
		}
	}
}

