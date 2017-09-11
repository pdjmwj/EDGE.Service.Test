using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zlib;

namespace EDGE.Common
{
	// Imported from the MetaFramework v110

	#region AutherializeFlags
	/// <summary>
	/// Used internally to represent certain encoding
	/// variables within the packed security token.
	/// </summary>
	[Flags]
	public enum AutherializeFlags
	{
		/// <summary>
		/// No flags set.
		/// </summary>
		None = 0,

		/// <summary>
		/// Content was compressed with Deflate algorithm.  This is only enabled
		/// if the content was actually smaller after compression.
		/// </summary>
		Deflated = 1,

		/// <summary>
		/// Content contains an expiration date (since it is stored outside of the
		/// key/value pairs as packed binary).
		/// </summary>
		ExpireDate = 2,

		/// <summary>
		/// Content was encrypted in CBC mode with PKCS7 padding.  This is only
		/// used if there is less than one block of data to transform, as it
		/// expands the original content.
		/// </summary>
		EncryptedCBC = 4,

		/// <summary>
		/// Content was encrypted either in CBC mode with no padding (if the content
		/// was an exact multiple of block size), or in CTS mode; plaintext and
		/// ciphertext are the same size.
		/// </summary>
		EncryptedCTS = 8,

		/// <summary>
		/// Deflate "suffix" bytes were stripped off.
		/// </summary>
		DeflateSuffix = 16
	}
	#endregion

	#region Autherialization Exceptions
	/// <summary>
	/// Represents a general-purpose exception when encoding or decoding Autherialized data.
	/// </summary>
	[Serializable]
	public class AutherializationException : Exception
	{
		/// <summary/>
		public AutherializationException(string msg) : base(msg) { }
	}
	/// <summary>
	/// Represents an attempt to decode a Autherialized data that is invalid or tampered.
	/// </summary>
	[Serializable]
	public class AutherializationExpiredException : AutherializationException
	{
		/// <summary/>
		public AutherializationExpiredException(string msg) : base(msg) { }
	}
	/// <summary>
	/// Represents an attempt to decode a Autherialized data that is expired.
	/// </summary>
	[Serializable]
	public class AutherializationInvalidException : AutherializationException
	{
		/// <summary/>
		public AutherializationInvalidException(string msg) : base(msg) { }
	}
	#endregion

	/// <summary>
	/// Extension methods for cryptography and data security operations.
	/// Includes the "Autherialize" methods providing a shared-secret-based
	/// authenticated, private serialization format.
	/// </summary>
	public static class DataSecurityExtensions
	{
		// General Data-Processing Extensions

		#region Base64WebSafeEncode
		/// <summary>
		/// Converts a byte array into a "web-safe" Base64 encoding.  This
		/// is the same as standard Base64 encoding, but with the /+= replaced
		/// with -._, making it safe for insertion into a URL without encoding,
		/// and protecting against tripping the ASP.NET "dangerous request"
		/// exception triggered by input values matching m/on[a-z]*=/i, which
		/// can happen if you put regular Base64 in any form variable, cookie,
		/// querystring, etc.  Use Base64WebSafeDecode to reverse the transformation.
		/// </summary>
		/// <param name="input">Bytes to encode.</param>
		/// <returns>Bytes encoded as a string.</returns>
		public static string Base64WebSafeEncode(this byte[] input)
		{
			if ( input == null )
				return null;
			return Convert.ToBase64String(input)
				.Replace("/", "-")
				.Replace("+", ".")
				.Replace("=", "_");
		}
		#endregion

		#region Base64WebSafeDecode
		/// <summary>
		/// Converts a string that was encoded with Base64WebSafeEncode back to
		/// the original byte array.
		/// </summary>
		/// <param name="input">String to decode.</param>
		/// <returns>Bytes that were encoded in the string.</returns>
		public static byte[] Base64WebSafeDecode(this string input)
		{
			if ( input == null )
				return null;
			return Convert.FromBase64String(input
				.Replace("-", "/")
				.Replace(".", "+")
				.Replace("_", "="));
		}
		#endregion

		#region GetUTF8Bytes
		/// <summary>
		/// Converts a string to its UTF-8 byte representation.
		/// </summary>
		public static byte[] GetUTF8Bytes(this string stringValue)
		{
			return Encoding.UTF8.GetBytes(stringValue);
		}
		#endregion

		#region DeflateCompress
		/// <summary>
		/// Compresses a series of bytes using the Deflate algorithm.
		/// </summary>
		public static IEnumerable<byte> DeflateCompress(this IEnumerable<byte> bytes, CompressionLevel level = CompressionLevel.Default)
		{
			byte[] ByteArray = bytes.ToArray();
			using ( MemoryStream R = new MemoryStream() )
			{
				R.Write(ByteArray, 0, ByteArray.Length);
				R.Seek(0, SeekOrigin.Begin);
				using ( MemoryStream W = new MemoryStream() )
				{
					using ( DeflateStream D = new DeflateStream(W, CompressionMode.Compress, level, true) )
					{
						R.CopyTo(D);
						D.Flush();
					}
					W.Seek(0, SeekOrigin.Begin);
					ByteArray = new byte[W.Length];
					W.Read(ByteArray, 0, ByteArray.Length);
				}
			}
			return ByteArray;
		}
		#endregion

		#region DeflateDecompress
		/// <summary>
		/// Decompresses a series of bytes using the Deflate algorithm.
		/// </summary>
		public static IEnumerable<byte> DeflateDecompress(this IEnumerable<byte> bytes)
		{
			byte[] ByteArray = bytes.ToArray();
			using ( MemoryStream R = new MemoryStream() )
			{
				R.Write(ByteArray, 0, ByteArray.Length);
				R.Seek(0, SeekOrigin.Begin);
				using ( MemoryStream W = new MemoryStream() )
				{
					using ( DeflateStream D = new DeflateStream(R, CompressionMode.Decompress, true) )
						D.CopyTo(W);
					W.Seek(0, SeekOrigin.Begin);
					ByteArray = new byte[W.Length];
					W.Read(ByteArray, 0, ByteArray.Length);
				}
			}
			return ByteArray;
		}
		#endregion

		#region TimingSafeEquals
		/// <summary>
		/// Compare 2 byte arrays (e.g. HMAC's) in a constant-time manner.  The
		/// standard "naive" algorithm (i.e. System.Collections.StructuralComparisons.StructuralComparer.Equals())
		/// will return early on the first different byte found in the array.  This leaks information about
		/// the first byte that differs between 2 arrays.  Where one is a user input, and the other is some
		/// "expected" value, this can be used to recover the "expected" value by manipulating the input and
		/// watching for timing variations.  This implementation guards against that.
		/// </summary>
		/// <remarks>
		/// A string variety is not provided as: (1) this algorithm does not guard against length differences,
		/// and the length of a string is generally not public knowledge; (2) much string data to compare
		/// is actually byte data (e.g. Base64-encoded), and could be decoded first before compare; (3) to
		/// compare strings A and B, one could simply do Encoding.UTF8.GetBytes(A).TimingSafeEquals(Encoding.UTF8.GetBytes(B)).
		/// </remarks>
		/// <param name="first">First byte array to compare.</param>
		/// <param name="second">Second byte array to compare.</param>
		/// <returns>True if the byte arrays are equal, false if there are any differences.</returns>
		public static bool TimingSafeEquals(this byte[] first, byte[] second)
		{
			// Trivial nullity cases.
			if ( first == null )
				return second == null;
			else if ( second == null )
				return false;

			// Note that if the byte arrays are not the same length, we effectively have to bail
			// out early, since it's impossible to hide length-based timing differences and still have
			// this method work correctly in all cases.  We assume that the length of the "expected" value
			// would be public knowledge so we don't need to guard against its leakage.
			if ( first.Length != second.Length )
				return false;

			// Accumulate all differences across all bytes in the arrays, but always walk through
			// the entire set of data.  Bailing out early would indicate to a user how far into the array
			// values had been "guessed" correctly, allowing them to attack an HMAC one byte at a time
			// and recover the correct HMAC after 256*length samples.
			int Compare = 0;
			for ( int I = 0; I < first.Length; I++ )
				Compare |= first[I] ^ second[I];
			return Compare == 0;
		}
		#endregion

		// Primitive Cryptographic Extensions

		#region Byte[] Hash Functions
		/// <summary>
		/// Get the hash as bytes.
		/// </summary>
		public static byte[] GetHashBytes<THash>(this byte[] binaryValue)
			where THash : HashAlgorithm, new()
		{
			return new THash().ComputeHash(binaryValue);
		}
		/// <summary>
		/// Get the HMAC as bytes.
		/// </summary>
		public static byte[] GetHMACBytes<THash>(this byte[] binaryValue, byte[] hMACKey)
			where THash : KeyedHashAlgorithm, new()
		{
			THash Hash = new THash();
			Hash.Key = hMACKey;
			return Hash.ComputeHash(binaryValue);
		}
		/// <summary>
		/// Get the HMAC as bytes.
		/// </summary>
		public static byte[] GetHMACBytes<THash>(this byte[] binaryValue, string hMACKey)
			where THash : KeyedHashAlgorithm, new()
		{
			return GetHMACBytes<THash>(binaryValue, GetUTF8Bytes(hMACKey));
		}
		#endregion

		#region String Hash Functions
		/// <summary>
		/// Get the hash as bytes.
		/// </summary>
		public static byte[] GetHashBytes<THash>(this string stringValue)
			where THash : HashAlgorithm, new()
		{
			return GetHashBytes<THash>(GetUTF8Bytes(stringValue));
		}
		/// <summary>
		/// Get the HMAC as bytes.
		/// </summary>
		public static byte[] GetHMACBytes<THash>(this string stringValue, byte[] hMACKey)
			where THash : KeyedHashAlgorithm, new()
		{
			return GetHMACBytes<THash>(GetUTF8Bytes(stringValue), hMACKey);
		}
		/// <summary>
		/// Get the HMAC as bytes.
		/// </summary>
		public static byte[] GetHMACBytes<THash>(this string stringValue, string hMACKey)
			where THash : KeyedHashAlgorithm, new()
		{
			return GetHMACBytes<THash>(GetUTF8Bytes(stringValue), GetUTF8Bytes(hMACKey));
		}
		#endregion

		#region CBCEncrypt
		/// <summary>
		/// Encrypts plaintext with a symmetric block cipher in CBC mode, with
		/// PKCS7 padding.  Up to one block size of overhead is added to the
		/// ciphertext.  Works with input of any size.
		/// </summary>
		/// <param name="alg">Symmetric algorithm with which to encrypt.</param>
		/// <param name="plaintext">Content to be encrypted.</param>
		/// <returns>Encrypted content.</returns>
		public static byte[] CBCEncrypt(this SymmetricAlgorithm alg, byte[] plaintext)
		{
			byte[] OldIV = alg.IV;
			CipherMode OldMode = alg.Mode;
			PaddingMode OldPad = alg.Padding;
			try
			{
				alg.IV = Enumerable.Repeat((byte)0, alg.IV.Length).ToArray();
				alg.Mode = CipherMode.CBC;
				alg.Padding = PaddingMode.PKCS7;
				return alg.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);
			}
			finally
			{
				alg.IV = OldIV;
				alg.Mode = OldMode;
				alg.Padding = OldPad;
			}
		}
		#endregion

		#region CBCDecrypt
		/// <summary>
		/// Decrypts data encrypted with CBCEncrypt.
		/// </summary>
		/// <param name="alg">Symmetric algorithm with which to decrypt.</param>
		/// <param name="ciphertext">Content to be decrypted.</param>
		/// <returns>Decrypted content.</returns>
		public static byte[] CBCDecrypt(this SymmetricAlgorithm alg, byte[] ciphertext)
		{
			byte[] OldIV = alg.IV;
			CipherMode OldMode = alg.Mode;
			PaddingMode OldPad = alg.Padding;
			try
			{
				alg.IV = Enumerable.Repeat((byte)0, alg.IV.Length).ToArray();
				alg.Mode = CipherMode.CBC;
				alg.Padding = PaddingMode.PKCS7;
				return alg.CreateDecryptor().TransformFinalBlock(ciphertext, 0, ciphertext.Length);
			}
			finally
			{
				alg.IV = OldIV;
				alg.Mode = OldMode;
				alg.Padding = OldPad;
			}
		}
		#endregion

		#region CTSEncrypt
		/// <summary>
		/// Encrypts data with the given symmetric block cipher in CBC mode with
		/// ciphertext stealing.  Ciphertext stealing is only enabled if the input
		/// is not an exact multiple of the cipher block size, otherwise plain
		/// unpadded CBC is used.  A minimum of one block size of input is required.
		/// Ciphertext is the same size as plaintext (there is no overhead space).
		/// </summary>
		/// <remarks>
		/// http://en.wikipedia.org/wiki/Ciphertext_stealing#CBC_implementation_notes
		/// </remarks>
		/// <param name="alg">Symmetric algorithm with which to encrypt.</param>
		/// <param name="plaintext">Content to be encrypted.</param>
		/// <returns>Encrypted content.</returns>
		public static byte[] CTSEncrypt(this SymmetricAlgorithm alg, byte[] plaintext)
		{
			byte[] OldIV = alg.IV;
			CipherMode OldMode = alg.Mode;
			PaddingMode OldPad = alg.Padding;
			try
			{
				alg.IV = Enumerable.Repeat((byte)0, alg.IV.Length).ToArray();
				alg.Mode = CipherMode.CBC;
				alg.Padding = PaddingMode.None;

				int BlockSize = alg.CreateEncryptor().InputBlockSize;
				if ( alg.CreateEncryptor().OutputBlockSize != BlockSize )
					throw new Exception("Chosen algorithm for CTS encryption has non-matching input/output block sizes.");

				if ( plaintext.Length < BlockSize )
					throw new Exception("CTS encryption with given algorithm requires at least " + BlockSize.ToString() + " bytes of plaintext.");
				if ( (plaintext.Length % BlockSize) == 0 )
					return alg.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);

				alg.Padding = PaddingMode.Zeros;
				byte[] CipherText = alg.CreateEncryptor().TransformFinalBlock(plaintext, 0, plaintext.Length);

				int CipherLen = CipherText.Length;
				int LastBlockSize = plaintext.Length + BlockSize - CipherLen;
				return CipherText.Take(CipherLen - BlockSize * 2)
					.Concat(CipherText.Skip(CipherLen - BlockSize).Take(BlockSize))
					.Concat(CipherText.Skip(CipherLen - BlockSize * 2).Take(LastBlockSize))
					.ToArray();
			}
			finally
			{
				alg.IV = OldIV;
				alg.Mode = OldMode;
				alg.Padding = OldPad;
			}
		}
		#endregion

		#region CTSDecrypt
		/// <summary>
		/// Decrypts data encrypted with CTSEncrypt.
		/// </summary>
		/// <remarks>
		/// http://en.wikipedia.org/wiki/Ciphertext_stealing#CBC_implementation_notes
		/// </remarks>
		/// <param name="alg">Symmetric algorithm with which to decrypt.</param>
		/// <param name="ciphertext">Content to be decrypted.</param>
		/// <returns>Decrypted content.</returns>
		public static byte[] CTSDecrypt(this SymmetricAlgorithm alg, byte[] ciphertext)
		{
			byte[] OldIV = alg.IV;
			CipherMode OldMode = alg.Mode;
			PaddingMode OldPad = alg.Padding;
			try
			{
				alg.IV = Enumerable.Repeat((byte)0, alg.IV.Length).ToArray();
				alg.Mode = CipherMode.CBC;
				alg.Padding = PaddingMode.None;

				int BlockSize = alg.CreateDecryptor().InputBlockSize;
				if ( alg.CreateDecryptor().OutputBlockSize != BlockSize )
					throw new Exception("Chosen algorithm for CTS encryption has non-matching input/output block sizes.");

				if ( ciphertext.Length < BlockSize )
					throw new Exception("CTS decryption with given algorithm requires at least " + BlockSize.ToString() + " bytes of ciphertext.");
				if ( (ciphertext.Length % BlockSize) == 0 )
					return alg.CreateDecryptor().TransformFinalBlock(ciphertext, 0, ciphertext.Length);

				int LastBlock = (int)(ciphertext.Length / BlockSize) * BlockSize;
				int SecLastBlock = LastBlock - BlockSize;
				byte[] PadC = ciphertext.Skip(SecLastBlock).Take(BlockSize).ToArray();
				byte[] PadP = alg.CreateDecryptor().TransformFinalBlock(PadC, 0, PadC.Length);

				byte[] Padded = ciphertext.Take(SecLastBlock)
					.Concat(ciphertext.Skip(LastBlock))
					.Concat(PadP.Skip(ciphertext.Length % BlockSize))
					.Concat(ciphertext.Skip(SecLastBlock).Take(BlockSize))
					.ToArray();
				byte[] PlainText = alg.CreateDecryptor().TransformFinalBlock(Padded, 0, Padded.Length);

				return PlainText.Take(ciphertext.Length).ToArray();
			}
			finally
			{
				alg.IV = OldIV;
				alg.Mode = OldMode;
				alg.Padding = OldPad;
			}
		}
		#endregion

		// Autherialization Constants

		#region HMAC_SIZE
		/// <summary>
		/// Size, in bytes, of the HMAC to use for validation.  The internal
		/// HMAC algorithm is set to SHA-256, but we can truncate it because
		/// we don't really need more than 128 bits of security, and space is
		/// at a premium in token strings.
		/// </summary>
		public static int HMAC_SIZE = 16;
		#endregion

		#region DEFLATE_SUFFIX
		/// <summary>
		/// The fixed suffix bytes in deflate-compressed data.  These are added by the
		/// deflate algorithm to flush the bit buffer and realign the data to a byte
		/// boundary, then terminate the stream.  Deflate leaves them in place, despite
		/// their redundancy, as a weak integrity check; we strip them off for maximum
		/// compression.
		/// </summary>
		public static readonly byte[] DEFLATE_SUFFIX = new byte[] { 0, 0, 255, 255, 3, 0 };
		#endregion

		#region EPOCH
		/// <summary>
		/// Our proprietary Autherialization Epoch, on which all expiration dates are based when encoded in
		/// a token as a uint32 number of seconds after this date.
		/// </summary>
		public static readonly DateTime EPOCH = new DateTime(2014, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		#endregion

		// Autherialization Helpers

		#region CreateCipher
		private static bool? _UseManagedAES = null;
		/// <summary>
		/// Given the encryption key, create the cipher to be used to encrypt/decrypt
		/// the secure payload.
		/// </summary>
		private static SymmetricAlgorithm CreateCipher()
		{
			SymmetricAlgorithm Cipher = null;

			// Try to use AES through the CryptoServiceProvider, which should run a faster
			// native implementation instead of a managed one.  If that fails, e.g. due to
			// OS limitations, fall back to the managed implementation.  Permanently memoize
			// the use of managed/unmanaged, so we don't have to do the try/catch every time.
			if ( _UseManagedAES.HasValue )
			{
				if ( _UseManagedAES.Value )
					Cipher = new AesManaged();
				else
					Cipher = new AesCryptoServiceProvider();
			}
			else
				try
				{
					Cipher = new AesCryptoServiceProvider();
					_UseManagedAES = false;
				}
				catch
				{
					Cipher = new AesManaged();
					_UseManagedAES = true;
				}

			return Cipher;
		}
		#endregion

		#region DataContractFullName
		/// <summary>
		/// Try to compute a "full name" from all data contract class elements.
		/// This is essentially a full type name, but allowing for datacontract
		/// name substitution to allow for interop between code with different
		/// actual internal class names.
		/// </summary>
		private static string DataContractFullName(Type type)
		{
			Queue<Type> ContractTypes = new Queue<Type>();
			ContractTypes.Enqueue(type);
			List<string> ContractNames = new List<string>();
			while ( ContractTypes.Count > 0 )
			{
				Type QT = ContractTypes.Dequeue();
				DataContractAttribute DCA = type.GetCustomAttribute<DataContractAttribute>();
				ContractNames.Add((DCA != null)
					? ("DC:" + DCA.Namespace + ":" + DCA.Name)
					: ("T:" + QT.FullName));
				foreach ( Type G in QT.GetGenericArguments()
					.Where(A => !A.IsGenericParameter) )
					ContractTypes.Enqueue(G);
			}
			return string.Join("|", ContractNames);
		}
		#endregion

		// Autherialization Extensions

		#region DataContractAutherialize
		/// <summary>
		/// Serializes a DataContract-serializable object into an encrypted, authenticated token string.
		/// Autherialize provides strong guarantees of authenticity (an attacker cannot create a token that
		/// will be accepted by the deserializer without knowing the key), and weak guarantees of privacy
		/// (under most circumstances, the content is obscured to prevent an attacker from reading it
		/// without the key).  Autherialize also provides type-safety, ensuring that a token from one
		/// data type cannot be misinterpreted into another data type, and offers an optional expiration
		/// time.
		/// </summary>
		/// <remarks>
		/// Autherialize is a portmanteau of "authenticate" and "serialize."
		/// </remarks>
		/// <param name="obj">Object to be serialized.  It must be a DataContract, and cannot have
		/// any looping references (JSON DataContracts are always serialized as IsReference=false).
		/// Note that polymorphism is allowed, but discouraged, as only .NET can properly encode/decode
		/// those objects (the __type property violates the JSON spec as it's strictly ordered).</param>
		/// <param name="key">Secret key to use for encryption and MAC.  Should contain at least 128 bits
		/// of entropy for full security.  The same key will need to be used to decrypt/validate the
		/// token on the receiving side (shared secret), so any entity that can validate a token also
		/// is able to create them.</param>
		/// <param name="expires">Optional expiration time for the token; null or DateTime.MaxValue
		/// means the token will never expire.</param>
		/// <param name="typeOverride">Force the data to be serialized as a different type, e.g. as
		/// a parent type.  This is useful if you want to serialize a subclass but only expect to
		/// be able to access the parent type on the receiving end.</param>
		/// <param name="allowCompress">Enable compression of payload data.  This is disabled by
		/// default to protect against a CRIME attack (https://en.wikipedia.org/wiki/CRIME_%28security_exploit%29).
		/// Only enable if you are certain that the payload will never contain both sensitive internal
		/// secrets (e.g. internal keys or passwords) and user input.</param>
		public static string DataContractAutherialize<T>(this T obj, string key,
			DateTime? expires = null, Type typeOverride = null, bool allowCompress = false)
		{
			if ( string.IsNullOrEmpty(key) )
				throw new AutherializationException("Key cannot be blank.");
			typeOverride = typeOverride ?? typeof(T);
			AutherializeFlags Flags = AutherializeFlags.None;

			// Serialize the payload object into a JSON Data Contract.  This is the most compact
			// interoperable format available to us.
			byte[] Payload;
			using ( MemoryStream MS = new MemoryStream() )
			{
				new DataContractJsonSerializer(typeOverride ?? typeof(T)).WriteObject(MS, obj);
				Payload = new byte[MS.Length];
				MS.Position = 0;
				MS.Read(Payload, 0, Payload.Length);
			}

			// Determine if the payload is to have an expiration time; if so, encode it as an
			// integer number of seconds since an epoch date.  We use 2014 as the epoch, instead of
			// the standard Unix epoch of 1970, since it doesn't make sense to waste bits to support
			// past dates.  To future-proof this, we're using a variable number of bytes to encode
			// the timestamp, up to 64 bits.
			if ( expires.HasValue && (expires < DateTime.MaxValue) )
			{
				ulong ExpStamp = (ulong)(expires.Value - EPOCH).TotalSeconds;
				if ( ExpStamp > 0 )
				{
					Flags |= AutherializeFlags.ExpireDate;
					byte IntFlag = 0;
					List<byte> StampBytes = new List<byte>();
					while ( ExpStamp > 0 )
					{
						StampBytes.Insert(0, (byte)(ExpStamp & 127 | IntFlag));
						ExpStamp >>= 7;
						IntFlag = 128;
					}
					Payload = StampBytes
						.Concat(Payload)
						.ToArray();
				}
			}

			// Try to compress the payload with the deflate algorithm, and keep the result if it's
			// smaller than the original.  Note that we check for a hard-coded suffix to the data, which
			// was observed empirically for certain inputs, and convert that to a flag too, to save
			// space, if possible.  This should help mitigate the cost of JSON encoding structures
			// in the data.  Note that encoding a token that contains both user input and internal
			// sensitive information will create a CRIME vulnerability
			// (https://en.wikipedia.org/wiki/CRIME_%28security_exploit%29).
			if ( allowCompress )
			{
				byte[] Compressed = Payload
					.DeflateCompress(Ionic.Zlib.CompressionLevel.BestCompression)
					.ToArray();
				AutherializeFlags DefSuff = AutherializeFlags.None;
				if ( (Compressed.Length > DEFLATE_SUFFIX.Length)
					&& StructuralComparisons.StructuralEqualityComparer.Equals(DEFLATE_SUFFIX,
					Compressed.Skip(Compressed.Length - DEFLATE_SUFFIX.Length).ToArray()) )
				{
					Compressed = Compressed.Take(Compressed.Length - DEFLATE_SUFFIX.Length).ToArray();
					DefSuff = AutherializeFlags.DeflateSuffix;
				}
				if ( Compressed.Length < Payload.Length )
				{
					Payload = Compressed;
					Flags |= AutherializeFlags.Deflated | DefSuff;
				}
			}

			// Derive encryption and HMAC keys from the developer-supplied key combined with
			// the full datacontract type name.  We're using the random oracle security model,
			// i.e. assuming that mathematical relationships between the keys via the HMAC
			// function are not exploitable.  This also means that if we try to deserialize a
			// token into a different type than that from which it was originally serialized,
			// it will fail, enforcing the data contract type match.
			// Note that AES-128 has about the same security level as AES-256 (due at least in
			// part to key scheduling weaknesses in AES-256) but AES-256 is about 40% slower,
			// and requires us to derive an additional HMAC-SHA256, so we'll split one 256-bit
			// HMAC to produce 2x 128-bit keys instead.
			string FullName = DataContractFullName(typeOverride);
			byte[] HMACKey = (FullName + ":HMAC").GetHMACBytes<HMACSHA256>(key);
			byte[] EncKeyRaw = (FullName + ":ENC").GetHMACBytes<HMACSHA256>(key);
			byte[] EncKey1 = new byte[16];
			Array.Copy(EncKeyRaw, 0, EncKey1, 0, 16);
			byte[] EncKey2 = new byte[16];
			Array.Copy(EncKeyRaw, 16, EncKey1, 0, 16);

			// Encrypt the payload content with the first encryption key.  The token was originally
			// designed to provide only authentication, not privacy, but I found developers using
			// it to carry sensitive information as if it provided privacy, so encryption was
			// added.  Use ciphertext stealing if the message is long enough, to avoid increasing
			// the size of the payload, otherwise use PKCS7 padding.
			SymmetricAlgorithm Cipher = CreateCipher();
			Cipher.Key = EncKey1;
			if ( Payload.Length < (Cipher.BlockSize / 8) )
			{
				Payload = Cipher.CBCEncrypt(Payload);
				Flags |= AutherializeFlags.EncryptedCBC;
			}
			else
			{
				Payload = Cipher.CTSEncrypt(Payload);
				Flags |= AutherializeFlags.EncryptedCTS;
			}

			// Reverse the payload and encrypt it again using a second key.  The first encryption
			// diffused each bit in the plaintext forward, and this reversed encryption then diffuses
			// it backwards, so that altering any bit in the plaintext will result in the pseudo-random
			// permutation of the entire ciphertext, not just the blocks following; this prevents
			// an attacker from determining the location within the message of any user-chosen plaintext,
			// without using an IV (which would increase the payload size).
			Cipher.Key = EncKey2;
			Payload = Cipher.CTSEncrypt(Payload
				.Reverse()
				.ToArray());

			// Prepend flags to the payload.
			Payload = new byte[] { (byte)Flags }
				.Concat(Payload)
				.ToArray();

			// Append an HMAC of the encrypted payload (encrypt-then-MAC).  Truncate the HMAC to
			// 128 bits, since we don't need a full 256 bits of security, and we can save on the
			// final token size here.
			Payload = Payload
				.Concat(Payload.GetHMACBytes<HMACSHA256>(HMACKey).Take(HMAC_SIZE))
				.ToArray();

			// Use a "web-safe" variant of base64 that can be included in a URL without needing to
			// be encoded, in case the token is naively included in a URL without encoding.
			return Payload.Base64WebSafeEncode();
		}
		#endregion

		#region DataContractDeAutherialize
		/// <summary>
		/// Deserializes a DataContract-serializable object from an encrypted, authenticated token string
		/// that was created using DataContractAutherialize.  The key and data type used must exactly
		/// match the originals used to Autherialize the original token.  Invalid or expired tokens will
		/// automatically be rejected, resulting in an exception.
		/// </summary>
		public static T DataContractDeAutherialize<T>(this string token, string key)
		{
			if ( string.IsNullOrEmpty(key) )
				throw new AutherializationException("Key cannot be blank.");
			if ( string.IsNullOrEmpty(token) )
				throw new AutherializationInvalidException("Token cannot be blank.");

			// Start decoding the input in reverse order from the way it was constructed.
			// See the DataContractAutherialize for reference.
			byte[] Payload = token.Base64WebSafeDecode();

			if ( Payload.Length < (HMAC_SIZE + 1) )
				throw new AutherializationInvalidException("Data is too short to be a valid secure token.");

			// Separate and validate the HMAC first.  Never try to decrypt a token that
			// hasn't already been authenticated, as trying to remove PKCS7 padding could
			// result in a padding oracle attack.
			// http://www.thoughtcrime.org/blog/the-cryptographic-doom-principle/
			// https://en.wikipedia.org/wiki/Padding_oracle_attack
			string FullName = DataContractFullName(typeof(T));
			byte[] HMACKey = (FullName + ":HMAC").GetHMACBytes<HMACSHA256>(key);
			byte[] HMAC = Payload
				.Skip(Payload.Length - HMAC_SIZE)
				.ToArray();
			Payload = Payload
				.Take(Payload.Length - HMAC_SIZE)
				.ToArray();

			// Note that we use a constant-time branchless algorithm for comparing the expected
			// HMAC against the included one.  Standard short-circuit-evaluation implementations
			// could leak information about the HMAC key, and/or provide an oracle to allow an attacker
			// to construct an HMAC for arbitrary byte arrays, through a timing side-channel.
			if ( !HMAC.TimingSafeEquals(Payload.GetHMACBytes<HMACSHA256>(HMACKey).Take(HMAC_SIZE).ToArray()) )
				throw new AutherializationInvalidException("HMAC validation failed.");

			// Split off the flags after authentication.
			AutherializeFlags Flags = (AutherializeFlags)Payload.First();
			Payload = Payload
				.Skip(1)
				.ToArray();

			// Handle payload encryption.
			if ( (Flags & (AutherializeFlags.EncryptedCBC | AutherializeFlags.EncryptedCTS)) != 0 )
			{
				byte[] EncKeyRaw = (FullName + ":ENC").GetHMACBytes<HMACSHA256>(key);
				byte[] EncKey1 = new byte[16];
				Array.Copy(EncKeyRaw, 0, EncKey1, 0, 16);
				byte[] EncKey2 = new byte[16];
				Array.Copy(EncKeyRaw, 16, EncKey1, 0, 16);
				SymmetricAlgorithm Cipher = CreateCipher();

				// If the payload was encrypted, it would have been double-encrypted, and
				// the outer encryption would always have been CBC-with-CTS.
				Cipher.Key = EncKey2;
				Payload = Cipher.CTSDecrypt(Payload)
					.Reverse()
					.ToArray();

				// The inner encryption would be either CBC or CBC-with-CTS encryption,
				// depending on the payload size and need for padding.
				Cipher.Key = EncKey1;
				if ( (Flags & AutherializeFlags.EncryptedCBC) != 0 )
					Payload = Cipher.CBCDecrypt(Payload);
				else if ( (Flags & AutherializeFlags.EncryptedCTS) != 0 )
					Payload = Cipher.CTSDecrypt(Payload);
			}

			// Decompress the payload, if applicable.
			if ( (Flags & AutherializeFlags.Deflated) != 0 )
			{
				if ( (Flags & AutherializeFlags.DeflateSuffix) != 0 )
					Payload = Payload.Concat(DEFLATE_SUFFIX)
						.ToArray();
				Payload = Payload
					.DeflateDecompress()
					.ToArray();
			}

			// Check for an expiration date, and invalidate the token if it's expired.
			if ( (Flags & AutherializeFlags.ExpireDate) != 0 )
			{
				List<byte> PayloadBytes = Payload.ToList();
				ulong ExpStamp = 0;
				while ( true )
				{
					byte B = PayloadBytes.First();
					PayloadBytes.RemoveAt(0);
					ExpStamp <<= 7;
					ExpStamp |= (ulong)B & 127;
					if ( (B & 128) == 0 )
						break;
				}
				DateTime Expired = EPOCH.AddSeconds(ExpStamp);
				if ( Expired < DateTime.UtcNow )
					throw new AutherializationExpiredException("Token has expired.");
				Payload = PayloadBytes.ToArray();
			}

			// Deserialize the embedded object and return it.
			using ( MemoryStream MS = new MemoryStream(Payload) )
			{
				object O = new DataContractJsonSerializer(typeof(T)).ReadObject(MS);
				return (O is T) ? (T)O : default(T);
			}
		}
		#endregion
	}
}