#region License
/*---------------------------------------------------------------------------------*\

	Distributed under the terms of an MIT-style license:

	The MIT License

	Copyright (c) 2006-2008 Stephen M. McKamey

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

namespace JsonFx.JsonML.Builder
{
	internal class JsonControl : IJsonControl, IEnumerable
	{
		#region Fields

		private string tagName;
		private Dictionary<String, Object> attributes = new Dictionary<String, Object>();
		private JsonControlCollection childControls;
		private JsonControl parent = null;

		#endregion Fields

		#region Init

		protected internal JsonControl() : this(String.Empty)
		{
		}

		public JsonControl(string tagName)
		{
			this.tagName = tagName;
			this.childControls = new JsonControlCollection(this);
		}

		#endregion Init

		#region Properties

		[JsonName("tag")]
		public string TagName
		{
			get { return this.tagName; }
			set { this.tagName = value; }
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
		public JsonControlCollection ChildControls
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

		[JsonIgnore]
		public JsonControl Parent
		{
			get { return this.parent; }
			internal set { this.parent = value; }
		}

		#endregion Properties

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new JsonControlEnumerator(this);
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

		private class JsonControlEnumerator : System.Collections.IEnumerator
		{
			#region Fields

			private JsonControl control;
			private EnumeratorState state = EnumeratorState.Start;
			private int index = 0;

			#endregion Fields

			#region Init

			public JsonControlEnumerator(JsonControl control)
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
