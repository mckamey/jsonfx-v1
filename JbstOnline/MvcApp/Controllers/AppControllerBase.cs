using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using JbstOnline.Mvc.ActionResults;
using Ninject;

namespace JbstOnline.Controllers
{
	public abstract class AppControllerBase :
		ControllerBase,
		IExceptionFilter
	{
		#region Properties

		/// <summary>
		/// Gets the ApiState for this request
		/// </summary>
		[Inject]
		public AppState ApiState
		{
			get;
			set;
		}

		#endregion Properties

		#region Error Handling Methods

		void IExceptionFilter.OnException(ExceptionContext context)
		{
			this.OnException(context);
		}

		protected virtual void OnException(ExceptionContext context)
		{
			context.Result = new ErrorResult(context.Exception);
			context.ExceptionHandled = true;
		}

		#endregion Error Handling Methods

		#region ControllerBase Members

		protected override void ExecuteCore()
		{
			RouteData routeData = this.ControllerContext == null ?
				null :
				this.ControllerContext.RouteData;

			IActionInvoker actionInvoker = this.ApiState.IoC.Get<IActionInvoker>();

			string actionName = routeData.GetRequiredString("action");
			if (!actionInvoker.InvokeAction(this.ControllerContext, actionName))
			{
				throw new HttpException(
					(int)HttpStatusCode.NotFound,
					String.Format("A public action method '{0}' could not be found on controller '{1}'.", actionName, this.GetType().FullName));
			}
		}

		#endregion ControllerBase Members
	}
}
