#region BuildTools License
/*---------------------------------------------------------------------------------*\

	BuildTools distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2007 Stephen M. McKamey

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in
	all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	THE SOFTWARE.

\*---------------------------------------------------------------------------------*/
#endregion BuildTools License

using System;

namespace BuildTools.HtmlDistiller.Filters
{
	/// <summary>
	/// Example HtmlFilter which limits size of images in the output
	/// </summary>
	public class ExampleHtmlFilter : IHtmlFilter
	{
		#region Fields

		private readonly int MaxImageSize;
		private readonly IHtmlFilter HtmlFilter;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="maxImageSize"></param>
		/// <remarks>
		/// Assumes SafeHtmlFilter characteristics
		/// </remarks>
		public ExampleHtmlFilter(int maxImageSize)
			: this(maxImageSize, null)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="maxImageSize"></param>
		/// <param name="filter"></param>
		public ExampleHtmlFilter(int maxImageSize, IHtmlFilter filter)
		{
			this.MaxImageSize = (maxImageSize > 0) ? maxImageSize : 0;
			this.HtmlFilter = (filter != null) ? filter : new SafeHtmlFilter();
		}

		#endregion Init

		#region IHtmlFilter Members

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public virtual bool FilterTag(HtmlTag tag)
		{
			// use this point to modify the tag output
			switch (tag.TagName)
			{
				case "img":
				{
					this.LimitImageSize(tag);
					break;
				}
			}

			return this.HtmlFilter.FilterTag(tag);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="attribute"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterAttribute(string tag, string attribute, ref string value)
		{
			return this.HtmlFilter.FilterAttribute(tag, attribute, ref value);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tag"></param>
		/// <param name="style"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public virtual bool FilterStyle(string tag, string style, ref string value)
		{
			return this.HtmlFilter.FilterStyle(tag, style, ref value);
		}

		#endregion IHtmlFilter Members

		#region Methods

		/// <summary>
		/// Limits the longest side of an image to MaxImageSize.
		/// </summary>
		/// <param name="tag"></param>
		private void LimitImageSize(HtmlTag tag)
		{
			if (tag.TagName != "img" || MaxImageSize <= 0)
			{
				return;
			}

			#region extract image dimensions

			int height = 0, width = 0;
			string strWidth = null, strHeight = null;

			if (tag.HasAttributes)
			{
				if (tag.Attributes.ContainsKey("height"))
				{
					strHeight = tag.Attributes["height"];
					tag.Attributes.Remove("height");
				}
				if (tag.Attributes.ContainsKey("width"))
				{
					strWidth = tag.Attributes["width"];
					tag.Attributes.Remove("width");
				}
			}

			if (tag.HasStyles)
			{
				if (String.IsNullOrEmpty(strHeight) &&
					tag.Styles.ContainsKey("height"))
				{
					strHeight = tag.Styles["height"];
				}
				if (String.IsNullOrEmpty(strWidth) &&
					tag.Styles.ContainsKey("width"))
				{
					strWidth = tag.Styles["width"];
				}
			}

			Int32.TryParse(strWidth, out width);
			Int32.TryParse(strHeight, out height);

			#endregion extract image dimensions

			#region calculate new dimensions

			int maxSide = Math.Max(height, width);

			if (height > 0 && width > 0)
			{
				if (maxSide > MaxImageSize)
				{
					if (height > width)
					{
						width = (MaxImageSize * width) / height;
						height = MaxImageSize;
					}
					else
					{
						height = (MaxImageSize * height) / width;
						width = MaxImageSize;
					}
				}
			}
			else
			{
				height = width = (maxSide <= 0) ? MaxImageSize : maxSide;
			}

			#endregion calculate new dimensions

			tag.Styles["height"] = height+"px";
			tag.Styles["width"] = width+"px";
		}

		#endregion Methods
	}
}
