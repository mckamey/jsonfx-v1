using System;
using System.Web.Mvc;

using Ninject.Modules;
using Ninject.Web.Mvc;

namespace JbstOnline.Mvc.IoC
{
	/// <summary>
	/// Establishes IoC bindings between DI interfaces and their implementations.
	/// </summary>
	public class AppIocModule : NinjectModule
	{
		#region NinjectModule Members

		public override void Load()
		{
			// MVC and IoC types
			this.Bind<IActionInvoker>().To<NinjectActionInvoker>().InTransientScope();

			// scope of ApiState is per request
			this.Bind<AppState>().ToSelf().InRequestScope();
		}

		#endregion NinjectModule Members
	}
}
