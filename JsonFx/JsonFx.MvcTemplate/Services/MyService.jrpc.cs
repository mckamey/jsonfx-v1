using System;
using System.Web;

using JsonFx.Json;
using JsonFx.JsonRpc;

namespace MyApp.Services
{
	[JsonService(Namespace="Example", Name="Service")]
	public class MyService
	{
		#region Service Methods

		/* these are the methods that are included in the JavaScript proxy */

		/// <summary>
		/// proxy function will be Example.Service.getInfo
		/// </summary>
		/// <param name="number">a number</param>
		/// <returns>MyServiceResult</returns>
		[JsonMethod(Name="getInfo")]
		public object GetInfo(double number)
		{
			return new
			{
				number = number,
				timestamp = this.GetTimeStamp(),
				machine = this.GetMachine()
			};
		}

		#endregion Service Methods

		#region Utility Methods

		/* these are not exposed in the JavaScript proxy since they do not have a JsonMethod attribute */

		public DateTime GetTimeStamp()
		{
			return DateTime.UtcNow;
		}

		public string GetMachine()
		{
			return HttpContext.Current.Server.MachineName;
		}

		#endregion Utility Methods
	}
}