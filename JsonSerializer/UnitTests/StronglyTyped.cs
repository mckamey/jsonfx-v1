using System;
using System.IO;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

using JsonFx.Json;

namespace BuildTools.Json.UnitTests
{
	/* A set of objects used to test strongly-typed serialization */

	public class StronglyTyped
	{
		#region Constants

		public const string MyTypeHintName = "__type";
		public const string UnitTestFile = "StronglyTyped.json";
		private const string Separator = "________________________________________\r\n";

		#endregion Constants

		#region Methods

		public static void RunTest(TextWriter writer, string unitTestsFolder, string outputFolder)
		{
			writer.WriteLine(Separator);

			ComplexObject collectionTest = new ComplexObject();

			collectionTest.MyNested = new NestedObject();
			collectionTest.MyNested.Items = new Dictionary<string, object>();
			collectionTest.MyNested.Items["First"] = 3.14159;
			collectionTest.MyNested.Items["Second"] = "Hello World";
			collectionTest.MyNested.Items["Third"] = 42;
			collectionTest.MyNested.Items["Fourth"] = true;

			collectionTest.MyNested.Hash = new Hashtable(collectionTest.MyNested.Items);

			collectionTest.MyNested.Hybrid = new HybridDictionary();
			foreach (string key in collectionTest.MyNested.Items.Keys)
			{
				collectionTest.MyNested.Hybrid[key] = collectionTest.MyNested.Items[key];
			}

			// populate with an Array
			collectionTest.MyArray = new SimpleObject[]{
						new SimpleObject(BlahBlah.Four),
						new SimpleObject(BlahBlah.Three),
						new SimpleObject(BlahBlah.Two),
						new SimpleObject(BlahBlah.One),
						new SimpleObject()
					};

			// duplicate for ArrayList
			collectionTest.MyArrayList = new ArrayList(collectionTest.MyArray);

			// duplicate for List<T>
			collectionTest.MyList = new List<SimpleObject>(collectionTest.MyArray);

			// duplicate for LinkedList<T>
			collectionTest.MyLinkedList = new LinkedList<SimpleObject>(collectionTest.MyArray);

			// duplicate for Stack<T>
			collectionTest.MyStack = new Stack<SimpleObject>(collectionTest.MyArray);

			// duplicate for Queue<T>
			collectionTest.MyQueue = new Queue<SimpleObject>(collectionTest.MyArray);

			using (JsonWriter jsonWriter = new JsonWriter(unitTestsFolder+UnitTestFile))
			{
				jsonWriter.TypeHintName = MyTypeHintName;
				jsonWriter.PrettyPrint = true;
				jsonWriter.Write(collectionTest);
			}

			string source = File.ReadAllText(unitTestsFolder+UnitTestFile);
			JsonReader jsonReader = new JsonReader(source);
			try
			{
				jsonReader.TypeHintName = MyTypeHintName;
				collectionTest = (ComplexObject)jsonReader.Deserialize(typeof(ComplexObject));
				writer.WriteLine("PASSED: "+UnitTestFile);
				writer.WriteLine("Result: {0}", (collectionTest == null) ? "null" : collectionTest.GetType().Name);
			}
			catch (JsonSerializationException ex)
			{
				int col, line;
				ex.GetLineAndColumn(source, out col, out line);

				writer.WriteLine("FAILED: StronglyTyped");
				writer.WriteLine("\"{0}\" ({1}, {2})", ex.Message, line, col);
			}
		}

		#endregion Methods
	}

	public class ComplexObject
	{
		#region Fields

		private Decimal myDecimal = 0.12345678901234567890123456789m;
		private Guid myGuid = Guid.NewGuid();
		private TimeSpan myTimeSpan = new TimeSpan(5, 4, 3, 2, 1);
		private Version myVersion = new Version(1, 2, 3, 4);
		private Uri myUri = new Uri("http://jsonfx.net/BuildTools");
		private DateTime myDateTime = DateTime.UtcNow;
		private Nullable<Int32> myNullableInt32 = null;
		private Nullable<Int64> myNullableInt64a = null;
		private Nullable<Int64> myNullableInt64b = 42;
		private Nullable<DateTime> myNullableDateTime = DateTime.Now;
		private SimpleObject[] myArray = null;
		private ArrayList myArrayList = null;
		private List<SimpleObject> myList = null;
		private LinkedList<SimpleObject> myLinkedList = null;
		private Stack<SimpleObject> myStack = null;
		private Queue<SimpleObject> myQueue = null;
		private NestedObject myNested = null;

		#endregion Fields

		#region Properties

		[JsonName("AnArbitraryRenameForMyNestedProperty")]
		public NestedObject MyNested
		{
			get { return this.myNested; }
			set { this.myNested = value; }
		}

		public Decimal MyDecimal
		{
			get { return this.myDecimal; }
			set { this.myDecimal = value; }
		}

		public Guid MyGuid
		{
			get { return this.myGuid; }
			set { this.myGuid = value; }
		}

		public TimeSpan MyTimeSpan
		{
			get { return this.myTimeSpan; }
			set { this.myTimeSpan = value; }
		}

		public Version MyVersion
		{
			get { return this.myVersion; }
			set { this.myVersion = value; }
		}

		public Uri MyUri
		{
			get { return this.myUri; }
			set { this.myUri = value; }
		}

		public DateTime MyDateTime
		{
			get { return this.myDateTime; }
			set { this.myDateTime = value; }
		}

		public Nullable<Int32> MyNullableInt32
		{
			get { return this.myNullableInt32; }
			set { this.myNullableInt32 = value; }
		}

		public Nullable<DateTime> MyNullableDateTime
		{
			get { return this.myNullableDateTime; }
			set { this.myNullableDateTime = value; }
		}

		[DefaultValue(null)]
		public Nullable<Int64> MyNullableInt64a
		{
			get { return this.myNullableInt64a; }
			set { this.myNullableInt64a = value; }
		}

		[DefaultValue(null)]
		public Nullable<Int64> MyNullableInt64b
		{
			get { return this.myNullableInt64b; }
			set { this.myNullableInt64b = value; }
		}

		public SimpleObject[] MyArray
		{
			get { return this.myArray; }
			set { this.myArray = value; }
		}

		public ArrayList MyArrayList
		{
			get { return this.myArrayList; }
			set { this.myArrayList = value; }
		}

		public List<SimpleObject> MyList
		{
			get { return this.myList; }
			set { this.myList = value; }
		}

		[JsonSpecifiedProperty("SerializeMyStack")]
		public Stack<SimpleObject> MyStack
		{
			get { return this.myStack; }
			set { this.myStack = value; }
		}

		[JsonIgnore]
		public bool SerializeMyStack
		{
			get { return false; }
			set { /* do nothing */ }
		}

		public LinkedList<SimpleObject> MyLinkedList
		{
			get { return this.myLinkedList; }
			set { this.myLinkedList = value; }
		}

		public Queue<SimpleObject> MyQueue
		{
			get { return this.myQueue; }
			set { this.myQueue = value; }
		}

		#endregion Properties
	}

	public class NestedObject
	{
		#region Fields

		private Dictionary<string, object> items = null;
		private Hashtable hash = null;
		private HybridDictionary hybrid = null;

		#endregion Fields

		#region Properties

		public Dictionary<string, object> Items
		{
			get { return this.items; }
			set { this.items = value; }
		}

		public Hashtable Hash
		{
			get { return this.hash; }
			set { this.hash = value; }
		}

		public HybridDictionary Hybrid
		{
			get { return this.hybrid; }
			set { this.hybrid = value; }
		}

		#endregion Properties
	}

	public class SimpleObject
	{
		#region Constants

		private static readonly Random Rand = new Random();

		#endregion Constants

		#region Fields

		double random;
		BlahBlah blah = BlahBlah.None;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		public SimpleObject()
		{
			this.random = Rand.NextDouble();
		}

		/// <summary>
		/// Ctor
		/// </summary>
		public SimpleObject(BlahBlah blah) : this()
		{
			this.Blah = blah;
		}

		#endregion Init

		#region Properties

		public BlahBlah Blah
		{
			get { return this.blah; }
			set { this.blah = value; }
		}

		public double Random
		{
			get { return this.random; }
			set { this.random = value; }
		}

		#endregion Properties

		#region Object Overrides

		public override string ToString()
		{
			return String.Format(
				"SimpleObject: {0}, {1}",
				this.Blah,
				this.Random);
		}

		#endregion Object Overrides
	}

	public enum BlahBlah
	{
		None,
		One,
		Two,
		Three,
		Four
	}
}
