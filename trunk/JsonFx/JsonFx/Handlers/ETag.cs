using System;
using System.IO;
using System.Text;
using System.Net;
using System.Security.Cryptography;

namespace JsonFx.Handlers
{
	/// <summary>
	/// Generates an HTTP/1.1 Cache header Entity Tag (ETag)
	/// </summary>
	/// <remarks>
	/// HTTP RFC: http://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html#sec14.19
	/// </remarks>
	public abstract class ETag
	{
		#region Constants

		public const HttpResponseHeader HttpHeader = HttpResponseHeader.ETag;

		#endregion Constants

		#region Fields

		private readonly string value;

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="entity"></param>
		public ETag(object entity)
		{
			this.value = this.Generator.GetETag(entity);
		}

		#endregion Init

		#region Properties

		/// <summary>
		/// Gets the algorithm to use for generating the ETag
		/// </summary>
		protected abstract IETagAlgorithm Generator
		{
			get;
		}

		/// <summary>
		/// Gets the ETag value for the associated entity
		/// </summary>
		public string Value
		{
			get { return this.value; }
		}

		#endregion Properties

		#region Methods

		/// <summary>
		/// Allows ETag implementors to pre-process entity into metadata
		/// </summary>
		/// <param name="entity"></param>
		/// <returns>entity metadata</returns>
		protected virtual object GetMetaData(object entity)
		{
			return entity;
		}

		#endregion Methods
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

		private static MD5ETagAlgorithm MD5Hash = new MD5ETagAlgorithm();

		#endregion Fields

		#region Init

		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="fileName"></param>
		public FileETag(string fileName)
			: base(fileName)
		{
		}

		#endregion Init

		#region ETag Members

		/// <summary>
		/// Gets the algorithm to use for generating the ETag
		/// </summary>
		protected override IETagAlgorithm Generator
		{
			get { return FileETag.MD5Hash; }
		}

		/// <summary>
		/// Generates a unique ETag which changes when the file changes
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		protected override object GetMetaData(object entity)
		{
			string fileName = entity as string;
			if (String.IsNullOrEmpty(fileName) || !File.Exists(fileName))
			{
				return entity;
			}

			FileInfo info = new FileInfo(fileName);
			string value = info.FullName.ToLowerInvariant();
			value += ";"+info.Length.ToString();
			value += ";"+info.CreationTimeUtc.Ticks.ToString();
			value += ";"+info.LastWriteTimeUtc.Ticks.ToString();

			return value;
		}

		#endregion ETag Members
	}
	
	#region ETag Algorithms

	/// <summary>
	/// Provides an algorithm for generating an HTTP/1.1 Cache header Entity Tag (ETag)
	/// </summary>
	public interface IETagAlgorithm
	{
		/// <summary>
		/// The algorithm behind generating an ETag
		/// </summary>
		/// <param name="entity">the entity which the ETag represents</param>
		/// <returns>the value for the ETag</returns>
		string GetETag(object metaData);
	}

	/// <summary>
	/// Creates a Base64-encoded MD5-hash as the ETag
	/// </summary>
	public class MD5ETagAlgorithm : IETagAlgorithm
	{
		#region Fields

		private static readonly MD5 MD5Hash;

		#endregion Fields

		#region Init

		/// <summary>
		/// CCtor
		/// </summary>
		static MD5ETagAlgorithm()
		{
			MD5ETagAlgorithm.MD5Hash = MD5.Create();
		}

		#endregion Init

		#region IETagGenerator Members

		/// <summary>
		/// Generates a unique ETag as MD5 hash from string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public string GetETag(string metaData)
		{
			// get String as a Byte[]
			byte[] buffer = Encoding.Unicode.GetBytes(metaData);

			return this.GetETag(buffer);
		}

		/// <summary>
		/// Generates a unique ETag as MD5 hash from Stream
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public string GetETag(Stream metaData)
		{
			// generate MD5 hash
			byte[] hash = MD5ETagAlgorithm.MD5Hash.ComputeHash(metaData);

			// convert hash to Base64 string
			return this.Base64Encode(hash);
		}

		/// <summary>
		/// Generates a unique ETag as MD5 hash from byte[]
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public string GetETag(byte[] metaData)
		{
			// generate MD5 hash
			byte[] hash = MD5ETagAlgorithm.MD5Hash.ComputeHash(metaData);

			// convert hash to Base64 string
			return this.Base64Encode(hash);
		}

		/// <summary>
		/// Generates a unique ETag as MD5 hash
		/// </summary>
		/// <param name="buffer"></param>
		/// <returns></returns>
		public string GetETag(object metaData)
		{
			if (metaData is String)
			{
				return this.GetETag((String)metaData);
			}
			if (metaData is byte[])
			{
				return this.GetETag((byte[])metaData);
			}
			if (metaData is Stream)
			{
				return this.GetETag((Stream)metaData);
			}

			throw new NotSupportedException(this.GetType().Name+" expects type of entity to be String, Byte[], or Stream");
		}

		#endregion IETagAlgorithm Members

		#region Methods

		/// <summary>
		/// Converts Byte[] to String
		/// </summary>
		/// <param name="hash"></param>
		/// <returns></returns>
		private string Base64Encode(byte[] hash)
		{
			// convert hash to Base64 string
			string base64 = Convert.ToBase64String(hash, Base64FormattingOptions.None);

			// trim padding chars
			return base64.Trim('=');
		}

		#endregion Methods
	}

	#endregion ETag Algorithms
}
