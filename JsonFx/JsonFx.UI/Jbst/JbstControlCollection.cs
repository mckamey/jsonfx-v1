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
	/// Control collection for JsonML+BST nodes.
	/// </summary>
	internal class JbstControlCollection : ICollection<IJbstControl>
	{
		#region Fields

		private List<IJbstControl> controls = new List<IJbstControl>();
		private JbstControl owner;

		#endregion Fields

		#region Init

		public JbstControlCollection(JbstControl owner)
		{
			this.owner = owner;
		}

		#endregion Init

		#region Properties

		public JbstControl Owner
		{
			get { return this.owner; }
		}

		public IJbstControl this[int index]
		{
			get { return this.controls[index]; }
			set { this.controls[index] = value; }
		}

		public IJbstControl Last
		{
			get
			{
				if (this.controls.Count < 1)
					return null;

				return this.controls[this.controls.Count-1];
			}
		}

		#endregion Properties

		#region ICollection<IJbstControl> Members

		public void Add(IJbstControl item)
		{
			this.controls.Add(item);
			if (item is JbstControl)
			{
				((JbstControl)item).Parent = this.Owner;
			}
		}

		public void Clear()
		{
			this.controls.Clear();
		}

		bool ICollection<IJbstControl>.Contains(IJbstControl item)
		{
			return this.controls.Contains(item);
		}

		void ICollection<IJbstControl>.CopyTo(IJbstControl[] array, int arrayIndex)
		{
			this.controls.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return this.controls.Count; }
		}

		bool ICollection<IJbstControl>.IsReadOnly
		{
			get { return ((ICollection<IJbstControl>)this.controls).IsReadOnly; }
		}

		bool ICollection<IJbstControl>.Remove(IJbstControl item)
		{
			return this.controls.Remove(item);
		}

		#endregion ICollection<IJbstControl> Members

		#region IEnumerable<IJbstControl> Members

		IEnumerator<IJbstControl> IEnumerable<IJbstControl>.GetEnumerator()
		{
			return this.controls.GetEnumerator();
		}

		#endregion IEnumerable<IJbstControl> Members

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.controls.GetEnumerator();
		}

		#endregion IEnumerable Members
	}
}
