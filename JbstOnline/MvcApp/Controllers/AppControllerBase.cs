using System;
using System.Web.Mvc;

using JbstOnline.Mvc.ActionResults;
using JsonFx.Mvc;
using Ninject;

namespace JbstOnline.Controllers
{
	public abstract class AppControllerBase : LiteController
	{
		#region Properties

		/// <summary>
		/// Gets the ApiState for this request
		/// </summary>
		[Inject]
		public IKernel IoC
		{
			get;
			set;
		}

		#endregion Properties

		#region LiteController Methods

		protected override void OnException(ExceptionContext context)
		{
			context.Result = new ErrorResult(context.Exception);
			context.ExceptionHandled = true;
		}

		public override T Get<T>()
		{
			return this.IoC.Get<T>();
		}

		#endregion LiteController Methods
	}
}
