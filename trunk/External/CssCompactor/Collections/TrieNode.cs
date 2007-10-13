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
using System.Collections.Generic;

namespace BuildTools.Collections
{
	/// <summary>
	/// A generic node for building a Trie
	/// </summary>
	/// <typeparam name="TKey">the Type used for the node path</typeparam>
	/// <typeparam name="TValue">the Type used for the node value</typeparam>
	/// <remarks>
	/// http://en.wikipedia.org/wiki/Trie
	/// </remarks>
	public class TrieNode<TKey, TValue>
	{
		#region Fields

		private TValue value = default(TValue);
		private readonly IDictionary<TKey, TrieNode<TKey, TValue>> children;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public TrieNode() : this(-1)
		{
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="capacity"></param>
		public TrieNode(int capacity)
		{
			if (capacity < 1)
			{
				this.children = new Dictionary<TKey, TrieNode<TKey, TValue>>();
			}
			else
			{
				this.children = new Dictionary<TKey, TrieNode<TKey, TValue>>(capacity);
			}
		}

		#endregion Init

		#region Properties

		public TrieNode<TKey, TValue> this[TKey key]
		{
			get
			{
				if (!this.children.ContainsKey(key))
				{
					return null;
				}
				return this.children[key];
			}
			protected set { this.children[key] = value; }
		}

		public TValue Value
		{
			get { return this.value; }
			protected set
			{
				if (!EqualityComparer<TValue>.Default.Equals(this.value, default(TValue)))
				{
					throw new InvalidOperationException("Trie path collision: the value for TrieNode<"+value.GetType().Name+"> has already been assigned.");
				}
				this.value = value;
			}
		}

		public bool HasValue
		{
			get
			{
				return !EqualityComparer<TValue>.Default.Equals(this.value, default(TValue));
			}
		}

		#endregion Properties

		#region Methods

		public bool Contains(TKey key)
		{
			return this.children.ContainsKey(key);
		}

		#endregion Methods
	}
}
