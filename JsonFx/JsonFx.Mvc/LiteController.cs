using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace JsonFx.Mvc
{
	/// <summary>
	/// A light-weight Controller base for basis of a Layer Supertype.
	/// Encourages IoC and reduces clutter.
	/// </summary>
	public abstract class LiteController :
		ControllerBase,
		IExceptionFilter
	{
		#region IoC Container

		/// <summary>
		/// IoC Container convenience method
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public abstract T Get<T>();

		#endregion IoC Container

		#region Error Handling Methods

		void IExceptionFilter.OnException(ExceptionContext context)
		{
			this.OnException(context);
		}

		protected abstract void OnException(ExceptionContext context);

		#endregion Error Handling Methods

		#region ActionResult Methods

		protected virtual DataResult DataResult(object data)
		{
			return this.DataResult(data, HttpStatusCode.OK);
		}

		protected virtual DataResult DataResult(object data, HttpStatusCode status)
		{
			DataResult result = this.Get<DataResult>();

			result.Data = data;

			if (status != HttpStatusCode.OK)
			{
				result.HttpStatusCode = status;
			}

			return result;
		}

		protected ViewResult View()
		{
			return View(/*viewName*/ null, /*masterName*/ null, /*model*/ null);
		}

		protected ViewResult View(object model)
		{
			return View(/*viewName*/ null, /*masterName*/ null, model);
		}

		protected ViewResult View(string viewName)
		{
			return View(viewName, /*masterName*/ null, /*model*/ null);
		}

		protected ViewResult View(string viewName, object model)
		{
			return View(viewName, /*masterName*/ null, model);
		}

		protected virtual ViewResult View(string viewName, string masterName, object model)
		{
			if (model != null)
			{
				this.ViewData.Model = model;
			}

			return new ViewResult
			{
				ViewName = viewName,
				MasterName = masterName,
				ViewData = this.ViewData,
				TempData = this.TempData
			};
		}

		#endregion ActionResult Methods

		#region ControllerBase Members

		protected override void ExecuteCore()
		{
			RouteData routeData = this.ControllerContext == null ? null :
				this.ControllerContext.RouteData;

			IActionInvoker actionInvoker = this.Get<IActionInvoker>();

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
