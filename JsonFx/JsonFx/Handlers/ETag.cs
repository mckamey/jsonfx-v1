using System;
using System.IO;
using System.Text;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Web;

namespace JsonFx.Handlers
{
	/// <summary>
	/// Generates an HTTP/1.1 Cache header Entity Tag (ETag)
	/// </summary>
	/// <remarks>
	/// HTTP/1.1 RFC:
	/// http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.19
	/// http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.26
	/// </remarks>
	public abstract class ETag
	{
		#region Constants

		private static readonly string RequestHeader = "If-None-Match";
		private static readonly string ResponseHeader = "ETag";
		private static readonly int NotModified = (int)HttpStatusCode.NotModified;
		private static readonly MD5 MD5HashProvider = MD5.Create();

		#endregion Constants

		#region Fields

		private string value = null;

		#endregion Fields

		#region Properties

		/// <summary>
		/// Gets the ETag value for the associated entity
		/// </summary>
		public string Value
		{
			get
			{
				if (this.value == null)
				{
					this.value = this.CalculateETag();
				}
				return this.value;
			}
		}

		#endregion Properties

		#region Public Methods

		/// <summary>
		/// Verifies if the client has a cached copy of the resource.
		/// Sets up HttpResponse appropriately.
		/// Returns true if cached.
		/// </summary>
		/// <param name="context"></param>
		/// <returns>true if is cached</returns>
		public bool HandleETag(HttpContext context)
		{
			return this.HandleETag(context, false);
		}

		/// <summary>
		/// Verifies if the client has a cached copy of the resource.
		/// Sets up HttpResponse appropriately.
		/// Returns true if cached.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="forceRefresh"></param>
		/// <returns>true if is cached</returns>
		public bool HandleETag(HttpContext context, bool forceRefresh)
		{
			if (context == null)
			{
				throw new ArgumentNullException("request");
			}

			HttpRequest request = context.Request;
			if (request == null)
			{
				throw new ArgumentNullException("context.Request");
			}

			// check request ETag
			bool isCached = false;
			string header = request.Headers[ETag.RequestHeader];
			if (!forceRefresh && !String.IsNullOrEmpty(header))
			{
				string[] etags = header.Split(',');
				foreach (string etag in etags)
				{
					// Value is case-sensitive
					if (this.ETagsEqual(this.Value, etag))
					{
						isCached = true;
						break;
					}
				}
			}

			// setup response ETag
			HttpResponse response = context.Response;
			if (response == null)
			{
				throw new ArgumentNullException("context.Response");
			}

			// specify ETag
			response.AppendHeader(ETag.ResponseHeader, this.Value);
			//response.Cache.SetETag(this.Value);

			if (isCached)
			{
				response.ClearContent();
				response.StatusCode = ETag.NotModified;

				// this safely ends request without causing "Transfer-Encoding: Chunked" which chokes IE6
				context.ApplicationInstance.CompleteRequest();
			}

			return isCached;
		}

		#endregion Public Methods

		#region Methods

		/// <summary>
		/// Provides an algorithm for generating an HTTP/1.1 Cache header Entity Tag (ETag)
		/// </summary>
		/// <returns>the value used to generate the ETag</returns>
		/// <remarks>
		/// GetMetaData must return String, Byte[], or Stream
		/// </remarks>
		protected abstract object GetMetaData();

		/// <summary>
		/// Sets ETag.Value
		/// </summary>
		/// <param name="Entity"></param>
		private string CalculateETag()
		{
			object metaData = this.GetMetaData();

			string etag;
			if (metaData is string)
			{
				etag = ETag.MD5Hash((string)metaData);
			}
			else if (metaData is byte[])
			{
				etag = ETag.MD5Hash((byte[])metaData);
			}
			else if (metaData is Stream)
			{
				etag = ETag.MD5Hash((Stream)metaData);
			}
			else
			{
				throw new NotSupportedException("GetMetaData must return String, Byte[], or Stream");
			}

			return etag;
		}

		#endregion Methods

		#region Utility Methods

		/// <summary>
		/// see System.Web.StaticFileHandler
		/// </summary>
		/// <param name="etag1"></param>
		/// <param name="etag2"></param>
		/// <returns></returns>
		private bool ETagsEqual(string etag1, string etag2)
		{
			if (String.IsNullOrEmpty(etag1) || String.IsNullOrEmpty(etag2))
			{
				return false;
			}
			etag1 = etag1.Trim();
			etag2 = etag2.Trim();

			if (etag1.Equals("*") || etag2.Equals("*"))
				return true;

			if (etag1.StartsWith("W/"))
				etag1 = etag1.Substring(2);

			if (etag2.StartsWith("W/"))
				etag2 = etag2.Substring(2);

			return etag2.Equals(etag1);
		}

		/// <summary>
		/// Generates a unique MD5 hash from string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static string MD5Hash(string value)
		{
			// get String as a Byte[]
			byte[] buffer = Encoding.Unicode.GetBytes(value);

			return ETag.MD5Hash(buffer);
		}

		/// <summary>
		/// Generates a unique MD5 hash from Stream
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		protected static string MD5Hash(Stream value)
		{
			// generate MD5 hash
			byte[] hash = MD5HashProvider.ComputeHash(value);

			// convert hash to Base64 string
			return ETag.Base64Encode(hash);
		}

		/// <summary>
		/// Generates a unique MD5 hash from byte[]
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		protected static string MD5Hash(byte[] value)
		{
			// generate MD5 hash
			byte[] hash = MD5HashProvider.ComputeHash(value);

			// convert hash to Base64 string
			return ETag.Base64Encode(hash);
		}

		/// <summary>
		/// Converts Byte[] to trimed Base64 String
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		protected static string Base64Encode(byte[] hash)
		{
			// convert hash to Base64 string
			string base64 = Convert.ToBase64String(hash, Base64FormattingOptions.None);

			// trim value-less padding chars
			return base64.Trim('=');
		}

		#endregion Utility Methods
	}

	/// <summary>
	/// Generates an ETag for an arbitrary string.
	/// </summary>
	public class StringETag : ETag
	{
		#region Fields

		private readonly string Content;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="Content"></param>
		public StringETag(string content)
		{
			this.Content = content;
		}

		#endregion Init

		#region ETag Members

		/// <summary>
		/// Generates a unique ETag which changes when the Content changes
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override object GetMetaData()
		{
			return this.Content;
		}

		#endregion ETag Members
	}

	/// <summary>
	/// Represents an ETag for a file on disk
	/// </summary>
	/// <remarks>
	/// Generates a unique ETag which changes when the file changes
	/// </remarks>
	public class FileETag : ETag
	{
		#region Fields

		private readonly string fileName;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="fileName"></param>
		public FileETag(string fileName)
		{
			this.fileName = fileName;
		}

		#endregion Init

		#region ETag Members

		/// <summary>
		/// Generates a unique ETag which changes when the file changes
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override object GetMetaData()
		{
			if (String.IsNullOrEmpty(this.fileName) || !File.Exists(this.fileName))
			{
				throw new FileNotFoundException("ETag cannot be created for missing file", this.fileName);
			}

			FileInfo info = new FileInfo(this.fileName);
			string value = info.FullName.ToLowerInvariant();
			value += ";"+info.Length.ToString();
			value += ";"+info.CreationTimeUtc.Ticks.ToString();
			value += ";"+info.LastWriteTimeUtc.Ticks.ToString();

			return value;
		}

		#endregion ETag Members
	}

	/// <summary>
	/// Represents an ETag for a file on disk
	/// </summary>
	/// <remarks>
	/// Generates a unique ETag which changes when the file changes
	/// </remarks>
	public class EmbeddedResourceETag : ETag
	{
		#region Fields

		private readonly Assembly Assembly;
		private readonly string ResourceName;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="fileName"></param>
		public EmbeddedResourceETag(Assembly assembly, string resourceName)
		{
			this.Assembly = assembly;
			this.ResourceName = resourceName;
		}

		#endregion Init

		#region ETag Members

		/// <summary>
		/// Generates a unique ETag which changes when the assembly changes
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override object GetMetaData()
		{
			if (this.Assembly == null)
			{
				throw new NullReferenceException("ETag cannot be created for null Assembly");
			}

			if (String.IsNullOrEmpty(this.ResourceName))
			{
				throw new NullReferenceException("ETag cannot be created for empty ResourceName");
			}

			Hash hash = new Hash(this.Assembly);
			Byte[] hashcode = hash.MD5;

			string value = ETag.Base64Encode(hashcode);
			value += ";"+this.ResourceName;

			return value;
		}

		#endregion ETag Members
	}
}
