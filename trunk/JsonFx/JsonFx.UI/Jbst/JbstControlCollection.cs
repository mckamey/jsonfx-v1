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
using System.Collections.Generic;

namespace JsonFx.UI.Jbst
{
	/// <summary>
	/// Control collection for JBST nodes.
	/// </summary>
	internal class JbstControlCollection : ICollection<JbstControl>
	{
		#region Fields

		private List<JbstControl> controls = new List<JbstControl>();
		private JbstContainerControl owner;

		#endregion Fields

		#region Init

		public JbstControlCollection(JbstContainerControl owner)
		{
			this.owner = owner;
		}

		#endregion Init

		#region Properties

		public JbstContainerControl Owner
		{
			get { return this.owner; }
		}

		public JbstControl this[int index]
		{
			get { return this.controls[index]; }
			set { this.controls[index] = value; }
		}

		public JbstControl Last
		{
			get
			{
				if (this.controls.Count < 1)
					return null;

				return this.controls[this.controls.Count-1];
			}
		}

		#endregion Properties

		#region ICollection<JbstControlBase> Members

		public void Add(JbstControl item)
		{
			this.controls.Add(item);
			item.Parent = this.Owner;
		}

		public void Clear()
		{
			this.controls.Clear();
		}

		bool ICollection<JbstControl>.Contains(JbstControl item)
		{
			return this.controls.Contains(item);
		}

		void ICollection<JbstControl>.CopyTo(JbstControl[] array, int arrayIndex)
		{
			this.controls.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return this.controls.Count; }
		}

		bool ICollection<JbstControl>.IsReadOnly
		{
			get { return ((ICollection<JbstControl>)this.controls).IsReadOnly; }
		}

		bool ICollection<JbstControl>.Remove(JbstControl item)
		{
			return this.controls.Remove(item);
		}

		internal bool Remove(JbstControl item)
		{
			return this.controls.Remove(item);
		}

		#endregion ICollection<JbstControlBase> Members

		#region IEnumerable<JbstControlBase> Members

		IEnumerator<JbstControl> IEnumerable<JbstControl>.GetEnumerator()
		{
			return this.controls.GetEnumerator();
		}

		#endregion IEnumerable<JbstControlBase> Members

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.controls.GetEnumerator();
		}

		#endregion IEnumerable Members
	}
}
