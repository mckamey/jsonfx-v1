/*------------------------------------------------------*\
	Copyright (c) 2007 Stephen M. McKamey

	CssCompactor is open-source under The MIT License
	http://www.opensource.org/licenses/mit-license.php
\*------------------------------------------------------*/

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
		/// 
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
