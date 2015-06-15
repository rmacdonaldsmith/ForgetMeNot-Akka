using System;
using Nancy;

namespace ForgetMeNot.API.HTTP.Modules
{
	public class HealthModule : RootModule
	{
		public HealthModule ()
		{
			Get ["/health"] = _ => {
				//return a 200 OK if all is well, otherwise return something else
				//wire this in to service monitor?
				return HttpStatusCode.OK;
			};
		}
	}
}

