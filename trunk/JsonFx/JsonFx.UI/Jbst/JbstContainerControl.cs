#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2009 Stephen M. McKamey

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
#endregion License

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;

using JsonFx.Json;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Internal representation of a JBST element.
	/// </summary>
	internal class JbstContainerControl : JbstControl, IEnumerable
	{
		#region Constants

		public const string PrefixDelim = ":";

		#endregion Constants

		#region Fields

		private string prefix;
		private string tagName;
		private Dictionary<String, Object> attributes = new Dictionary<String, Object>();
		private JbstControlCollection childControls;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public JbstContainerControl()
			: this(String.Empty)
		{
		}

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="tagName"></param>
		public JbstContainerControl(string tagName)
		{
			this.childControls = new JbstControlCollection(this);

			if (tagName == null)
			{
				tagName = String.Empty;
			}

			this.prefix = SplitPrefix(tagName, out this.tagName);
		}

		#endregion Init

		#region Properties

		[JsonName("tagName")]
		public string TagName
		{
			get { return this.tagName; }
			set { this.tagName = value; }
		}

		[JsonName("prefix")]
		public string Prefix
		{
			get { return this.prefix; }
			set { this.prefix = value; }
		}

		[JsonName("rawName")]
		public virtual string RawName
		{
			get
			{
				if (String.IsNullOrEmpty(this.prefix))
				{
					return this.TagName;
				}
				return this.Prefix + PrefixDelim + this.TagName;
			}
		}

		[JsonName("attributes")]
		public Dictionary<String, Object> Attributes
		{
			get { return this.attributes; }
			set { this.attributes = value; }
		}

		[JsonIgnore]
		public bool AttributesSpecified
		{
			get { return this.Attributes.Keys.Count > 0; }
			set { }
		}

		//[JsonIgnore]
		//public Dictionary<String, Object> Style
		//{
		//    get
		//    {
		//        Dictionary<String, Object> style = this.Attributes["style"] as Dictionary<String, Object>;
		//        if (style == null)
		//        {
		//            style = new Dictionary<String, Object>();
		//            this.Attributes["style"] = style;
		//        }
		//        return style;
		//    }
		//}

		[JsonName("children")]
		public virtual JbstControlCollection ChildControls
		{
			get { return this.childControls; }
			set { this.childControls = value; }
		}

		[JsonIgnore]
		public bool ChildControlsSpecified
		{
			get { return this.ChildControls.Count > 0; }
			set { }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Splits the prefix and tag name
		/// </summary>
		/// <param name="rawName"></param>
		/// <param name="tagName"></param>
		/// <returns></returns>
		protected internal static string SplitPrefix(string rawName, out string tagName)
		{
			int index = rawName.IndexOf(PrefixDelim);
			if (index < 0)
			{
				tagName = rawName;
				return String.Empty;
			}
			else
			{
				tagName = rawName.Substring(index+1);
				return rawName.Substring(0, index);
			}
		}

		#endregion Methods

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new JbstControlEnumerator(this);
		}

		#endregion IEnumerable Members

		#region Enumerator State

		enum EnumeratorState : int
		{
			Start = 0,
			TagName = 1,
			Attributes = 2,
			ChildControls = 3,
			End = -1
		}

		#endregion Enumerator State

		/// <summary>
		/// An enumerator for JbstControl objects.
		/// </summary>
		private class JbstControlEnumerator : IEnumerator
		{
			#region Fields

			private JbstContainerControl control;
			private EnumeratorState state = EnumeratorState.Start;
			private int index = 0;

			#endregion Fields

			#region Init

			public JbstControlEnumerator(JbstContainerControl control)
			{
				this.control = control;
			}

			#endregion Init

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get
				{
					switch (this.state)
					{
						case EnumeratorState.TagName:
						{
							return this.control.TagName;
						}
						case EnumeratorState.Attributes:
						{
							return this.control.Attributes;
						}
						case EnumeratorState.ChildControls:
						{
							return this.control.ChildControls[this.index];
						}
						default:
						{
							throw new InvalidOperationException("Bad enumerator state.");
						}
					}
				}
			}

			bool System.Collections.IEnumerator.MoveNext()
			{
				// key off current state
				switch (this.state)
				{
					case EnumeratorState.Start:
					{
						this.state = EnumeratorState.TagName;
						return true;
					}
					case EnumeratorState.TagName:
					{
						if (!this.control.AttributesSpecified)
							goto case EnumeratorState.Attributes;

						this.state = EnumeratorState.Attributes;
						return true;
					}
					case EnumeratorState.Attributes:
					{
						if (!this.control.ChildControlsSpecified)
							goto case EnumeratorState.ChildControls;

						this.state = EnumeratorState.ChildControls;
						return true;
					}
					case EnumeratorState.ChildControls:
					{
						this.index++;
						if (this.index >= this.control.ChildControls.Count)
							goto default;

						return true;
					}
					default:
					{
						this.state = EnumeratorState.End;
						return false;
					}
				}
			}

			void System.Collections.IEnumerator.Reset()
			{
				this.state = EnumeratorState.Start;
			}

			#endregion IEnumerator Members
		}
	}
}
