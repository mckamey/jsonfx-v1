﻿using System;
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
		/// Gets the IoC
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

		protected override DataResult DataResult()
		{
			return this.IoC.Get<DataResult>();
		}

		protected override IActionInvoker ActionInvoker
		{
			get { return this.IoC.Get<IActionInvoker>(); }
			set {}
		}

		#endregion LiteController Methods
	}
}