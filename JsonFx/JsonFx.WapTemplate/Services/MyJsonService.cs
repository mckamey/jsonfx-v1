using System;
using System.Web;

using JsonFx.Json;
using JsonFx.JsonRpc;

namespace MyApp.Services
{
	[JsonService(Namespace="Example", Name="MyServiceProxy")]
	public class MyJsonService
	{
		#region Service Methods

		/* these are the methods that are included in the JavaScript proxy */

		/// <summary>
		/// proxy function will be Example.MyServiceProxy.getInfo
		/// </summary>
		/// <param name="number">a number</param>
		/// <returns>MyServiceResult</returns>
		[JsonMethod(Name="getInfo")]
		public MyServiceResult GetInfo(double number)
		{
			MyServiceResult result;

			result.Number = number;
			result.TimeStamp = this.GetTimeStamp();
			result.MachineName = this.GetMachine();

			return result;
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

	public struct MyServiceResult
	{
		[JsonName("timestamp")]
		public DateTime TimeStamp;

		[JsonName("number")]
		public double Number;

		[JsonName("machine")]
		public string MachineName;
	}
}