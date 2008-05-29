using System;
using System.Collections.Generic;

namespace JsonFx.JsonML.Builder
{
	internal class JsonControlCollection : ICollection<IJsonControl>
	{
		#region Fields

		private List<IJsonControl> controls = new List<IJsonControl>();
		private JsonControl owner;

		#endregion Fields

		#region Init

		public JsonControlCollection(JsonControl owner)
		{
			this.owner = owner;
		}

		#endregion Init

		#region Properties

		public JsonControl Owner
		{
			get { return this.owner; }
		}

		public IJsonControl this[int index]
		{
			get { return this.controls[index]; }
			set { this.controls[index] = value; }
		}

		public IJsonControl Last
		{
			get
			{
				if (this.controls.Count < 1)
					return null;

				return this.controls[this.controls.Count-1];
			}
		}

		#endregion Properties

		#region ICollection<IJsonControl> Members

		public void Add(IJsonControl item)
		{
			this.controls.Add(item);
			if (item is JsonControl)
			{
				((JsonControl)item).Parent = this.Owner;
			}
		}

		public void Clear()
		{
			this.controls.Clear();
		}

		bool ICollection<IJsonControl>.Contains(IJsonControl item)
		{
			return this.controls.Contains(item);
		}

		void ICollection<IJsonControl>.CopyTo(IJsonControl[] array, int arrayIndex)
		{
			this.controls.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return this.controls.Count; }
		}

		bool ICollection<IJsonControl>.IsReadOnly
		{
			get { return ((ICollection<IJsonControl>)this.controls).IsReadOnly; }
		}

		bool ICollection<IJsonControl>.Remove(IJsonControl item)
		{
			return this.controls.Remove(item);
		}

		#endregion ICollection<IJsonControl> Members

		#region IEnumerable<IJsonControl> Members

		IEnumerator<IJsonControl> IEnumerable<IJsonControl>.GetEnumerator()
		{
			return this.controls.GetEnumerator();
		}

		#endregion IEnumerable<IJsonControl> Members

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.controls.GetEnumerator();
		}

		#endregion IEnumerable Members
	}
}
