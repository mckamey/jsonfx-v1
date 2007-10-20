using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JsonFx.JSON
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

			if (String.IsNullOrEmpty(name))
				throw new InvalidOperationException("Invalid JsonObject property name.");
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
