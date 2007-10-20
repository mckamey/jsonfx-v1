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
using System.Xml.Serialization;

namespace JsonFx.Json
{
	/// <summary>
	/// Generalized object for containing untyped JSON data.
	/// </summary>
	public class JsonObject : System.Xml.Serialization.IXmlSerializable
	{
		#region Constants

		//protected const string Key_JsonClass = "__jsonclass__";

		#endregion Constants

		#region Fields

		private Dictionary<String, Object> properties;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor.
		/// </summary>
		public JsonObject()
		{
			this.properties = new Dictionary<String, Object>(StringComparer.InvariantCulture);
		}

		/// <summary>
		/// Ctor.
		/// </summary>
		/// <param name="propertyCount">the number of properties to reserve space for</param>
		public JsonObject(int propertyCount)
		{
			this.properties = new Dictionary<String, Object>(propertyCount, StringComparer.InvariantCulture);
		}

		#endregion Init

		#region Properties

		public object this[string name]
		{
			get
			{
				this.CheckPropertyName(name);
				if (!this.properties.ContainsKey(name))
					return null;

				return this.properties[name];
			}
			set
			{
				this.CheckPropertyName(name);
				this.properties[name] = value;
			}
		}

		protected internal ICollection<String> Properties
		{
			get { return this.properties.Keys; }
		}

		//public virtual string JsonClass
		//{
		//    get { return this[JsonObject.Key_JsonClass] as string; }
		//    set { this[JsonObject.Key_JsonClass] = value; }
		//}

		#endregion Properties

		#region Methods

		protected void CheckPropertyName(string name)
		{
			// need to check what the restrictions are on property names...

			if (name == null)
			{
				throw new InvalidOperationException("Invalid JSON object property name.");
			}
		}

		#endregion Methods

		#region IXmlSerializable Members

		System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
		{
			return null;
		}

		void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
		{
//TODO: implement deserialization from XML
		}

		void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
		{
			foreach (string property in this.Properties)
			{
				writer.WriteStartElement("Property");
				writer.WriteAttributeString("name", property);

				object value = this[property];
				if (value != null)
				{
					XmlSerializer serializer = new XmlSerializer(value.GetType());
					serializer.Serialize(writer, value);
				}

				writer.WriteEndElement();
			}
		}

		#endregion
	}
}
