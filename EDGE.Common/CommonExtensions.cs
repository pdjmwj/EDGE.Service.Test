using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace EDGE.Common
{
	public static class CommonExtensions
	{
		#region | Exception: FormatMessage |
		/// <summary>
		/// Standardize form error messages.
		/// </summary>
		public static string FormatMessage(this Exception ex)
		{
			return CommonExtensions.FormatMessage(ex, string.Empty);
		}
		public static string FormatMessage(this Exception ex, string routineName)
		{
			if ( ex == null )
				return string.Empty;

			var Msgs = new List<string>();

			if ( !routineName.IsEmpty() )
				Msgs.Add(routineName.Trim());

			// Loop thru Messages
			Exception HoldException = ex;
			var Str = new StringBuilder();
			while ( ex != null )
			{
				if ( !Msgs.Contains(ex.Message, StringComparison.CurrentCultureIgnoreCase) )
				{
					Msgs.Add(ex.Message);
					Str.AppendLine(ex.Message);
				}
				if ( !Msgs.Contains(ex.Source, StringComparison.CurrentCultureIgnoreCase) )
				{
					Msgs.Add(ex.Source);
					Str.AppendLine(ex.Source);
				}

				Msgs.Add(ex.StackTrace);
				Str.AppendLine(ex.StackTrace);

				ex = ex.InnerException;
			}

			return string.Format("Internal Error {0}", Str.ToString());
		}
		#endregion

		#region | Execution: FormatMessage |
		/// <summary>
		/// Standardize form error messages.
		/// </summary>
		public static string FormatMessage(this ExecutionMessageCollection messagesList, bool concatOneLine = false)
		{
			return CommonExtensions.FormatMessage(messagesList, string.Empty, concatOneLine);
		}
		public static string FormatMessage(this ExecutionMessageCollection messagesList, string routineName, bool concatOneLine = false)
		{
			if ( messagesList.Count == 0 )
				return string.Empty;

			var Msgs = new List<string>();

			if ( !routineName.IsEmpty() )
				Msgs.Add(routineName.Trim());

			// Loop thru Messages
			var Str = new StringBuilder();
			foreach ( ExecutionMessage Msg in messagesList )
			{
				string s = Msg.Text.Replace("PLEASE NOTE: ", "");
				if ( !Msgs.Contains(s, StringComparison.CurrentCultureIgnoreCase) )
				{
					Msgs.Add(s);
					if ( concatOneLine )
						Str.Append(string.Format("{0}{1}", Str.Length == 0 ? "" : " | ", s));
					else
						Str.AppendLine(s);
				}
			}

			return Str.ToString();
		}
		#endregion

		#region | object: InvokeUnlessDefault |
		/// <summary>
		/// Invoke an action if the input is not default.  For reference
		/// types, default means null.
		/// </summary>
		public static void InvokeUnlessDefault<TObj>(this TObj obj, Action<TObj> toRun)
		{
			if ( !object.Equals((object)obj, (object)default(TObj)) )
				toRun.Invoke(obj);
		}
		/// <summary>
		/// Invoke a function if the input is not its type's default value.
		/// For reference types, default means null.  Returns the result of
		/// the function if it was invoked, or the specified default value
		/// (or default for the result type) if not invoked.
		/// </summary>
		public static TResult InvokeUnlessDefault<TObj, TResult>(this TObj obj, Func<TObj, TResult> toRun, TResult defaultResult = default(TResult))
		{
			TResult Result = defaultResult;
			obj.InvokeUnlessDefault(O => { Result = toRun.Invoke(O); });
			return Result;
		}
		#endregion

		#region | enum: GetDesc |
		public static string GetDesc(this Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());

			DescriptionAttribute[] attributes =
				(DescriptionAttribute[])fi.GetCustomAttributes(
				typeof(DescriptionAttribute),
				false);

			if ( attributes != null &&
				attributes.Length > 0 )
				return attributes[0].Description;
			else
				return value.ToString();
		}
		#endregion

		#region | List<string>: Contains |
		/// <summary>
		/// Returns a value indicating whether a specified substring occurs within this string using case option.
		/// </summary>
		public static bool Contains(this List<string> source, string toCheck, StringComparison comp)
		{
			bool IsFound = false;
			if ( !toCheck.IsEmpty() )
			{
				foreach ( string s in source )
				{
					if ( s.Equals(toCheck, comp) )
					{
						IsFound = true;
						break;
					}
				}
			}
			return IsFound;
		}
		#endregion

		#region | string: Contains |
		/// <summary>
		/// Returns a value indicating whether a specified substring occurs within this string using case option.
		/// </summary>
		public static bool Contains(this string source, string toCheck, StringComparison comp)
		{
			if ( string.IsNullOrEmpty(source) || string.IsNullOrEmpty(toCheck) )
				return false;
			return source.IndexOf(toCheck, comp) >= 0;
		}
		#endregion

		#region | string: IsDate |
		/// <summary>
		/// Determines if a date is valid. 
		/// </summary>
		public static bool IsDate(this string value)
		{
			try
			{
				DateTime tmp = Convert.ToDateTime(value);
			}
			catch
			{
				return false;
			}
			return true;
		}
		#endregion

		#region | string: IsEmpty |
		/// <summary>
		/// Determines if a string is empty 
		/// </summary>
		public static bool IsEmpty(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}
		#endregion

		#region | object: IsNull |
		/// <summary>
		/// Determines if an object is Null
		/// </summary>
		public static bool IsNull(this object value)
		{
			return value == null;
		}
		#endregion

		#region | string: Replace |
		/// <summary>
		/// Replace a value in a string with comparison options
		/// </summary>
		public static string Replace(this string str, string oldValue, string newValue, StringComparison comparisonType)
		{
			int prevPos = 0;
			string retval = str;

			// find the first occurence of oldValue
			int pos = retval.IndexOf(oldValue, comparisonType);

			while ( pos > -1 )
			{
				// remove oldValue from the string
				retval = retval.Remove(pos, oldValue.Length);

				// insert newValue in it's place
				retval = retval.Insert(pos, newValue);

				// check if oldValue is found further down
				prevPos = pos + newValue.Length;
				pos = retval.IndexOf(oldValue, prevPos, comparisonType);
			}

			return retval;
		}
		#endregion

		#region | string: IsNumeric |
		/// <summary>
		/// Is the string numeric
		/// </summary>
		public static bool IsNumeric(this string value)
		{
			double Parsed;
			return Double.TryParse(value,
				System.Globalization.NumberStyles.Any,
				System.Globalization.NumberFormatInfo.InvariantInfo,
				out Parsed);
		}
		#endregion

		#region | string: ToEnum |
		public static T ToEnum<T>(this string value)
		{
			//return (T)Enum.Parse(typeof(T), value);

			Contract.Requires(typeof(T).IsEnum);
			Contract.Requires(value != null);
			Contract.Requires(Enum.IsDefined(typeof(T), value));
			return (T)Enum.Parse(typeof(T), value);
		}
		#endregion

		#region | string: ToTitleCase |
		/// <summary>
		/// Convert string to title case XXXXX => Xxxxxx
		/// </summary>
		public static string ToTitleCase(this string str)
		{
			CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
			TextInfo textInfo = cultureInfo.TextInfo;
			return textInfo.ToTitleCase(str);
		}
		#endregion

		#region | DateTime: ToMinDate |
		public static void ToMinDate(this DateTime value)
		{
			value = DateTime.MinValue;
		}
		#endregion

		#region | DateTime: ToStringDate |
		public static string ToStringDate(this DateTime value)
		{
			return value == DateTime.MinValue ? "" : value.ToString("MM/dd/yyyy");
		}
		#endregion

		#region | DateTime: ToStringDateTime |
		public static string ToStringDateTime(this DateTime value)
		{
			return value == DateTime.MinValue ? "" : value.ToString("MM/dd/yy hh:mm tt");
		}
		#endregion

		#region | object: ToStringOrEmpty |
		/// <summary>
		/// Convert an object or null to a string, never returning
		/// a null string.
		/// </summary>
		public static string ToStringOrEmpty(this object o)
		{
			try
			{
				if ( o == null )
					return string.Empty;
				return (o.ToString() ?? string.Empty).Trim();
			}
			catch
			{
				return string.Empty;
			}
		}
		#endregion

		#region | IEnumerable: ToHashSet |
		/// <summary>
		/// Wraps the enumerable in a HashSet.
		/// </summary>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> toHash)
		{
			return new HashSet<T>(toHash);
		}
		/// <summary>
		/// Wraps the enumerable in a HashSet.
		/// </summary>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> toHash, IEqualityComparer<T> comparer)
		{
			return new HashSet<T>(toHash, comparer);
		}
		#endregion

		#region | DataTable: ColumnRemove |
		public static void ColumnRemove(this DataTable dt, string columnName)
		{
			try
			{
				dt.Columns.Remove(columnName);
			}
			catch
			{
				// Do nothing.....
			}
		}
		#endregion

		#region | DataTable: ColumnRename |
		public static void ColumnRename(this DataTable dt, string oldColumnName, string newColumnName)
		{
			try
			{
				dt.Columns[oldColumnName].ColumnName = newColumnName;
				dt.AcceptChanges();
			}
			catch
			{
				// Do nothing.....
			}
		}
		#endregion

		#region | DataTable: ToList |
		/// <summary>
		/// Return a list of T types from the DataTable.
		/// </summary>
		/// <remarks>
		/// Any Property in the type with a matching column name in the
		/// DataTable is populated in the object list returned.
		/// </remarks>
		/// <typeparam name="T">The type of object to return.</typeparam>
		/// <param name="dt">Source DataTable</param>
		/// <param name="objAction">Optional action to be performed on each object after
		/// it has been created, but before it is added to the list.</param>
		/// <returns>List of T objects.</returns>
		public static List<T> ToList<T>(this DataTable dt, Action<T> objAction = null) where T : new()
		{
			List<T> TR = new List<T>();

			// Get mapping of properties to columns that we want to convert.
			var PI = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(P => P.CanWrite && !P.GetIndexParameters().Any() && dt.Columns.Contains(P.Name))
				.Select(P => new Tuple<PropertyInfo, int>(P, dt.Columns.IndexOf(P.Name)))
				.ToList();

			foreach ( DataRow DR in dt.Rows )
			{
				T NewObj = new T();
				PI.ForEach((P) =>
				{
					// Got to do some strange stuff for DBNull values in the DataTable
					if ( P.Item1.PropertyType.IsGenericType && P.Item1.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) )
					{
						dynamic objValue = System.Activator.CreateInstance(P.Item1.PropertyType);
						objValue = Convert.IsDBNull(DR[P.Item2]) ? null : DR[P.Item2];
						P.Item1.SetValue(NewObj, (object)objValue, null);
					}
					else
					{
						P.Item1.SetValue(NewObj, Convert.IsDBNull(DR[P.Item2]) ? null : DR[P.Item2], null);
					}
				});

				// Perform custom action on the new object
				if ( objAction != null )
					objAction.Invoke(NewObj);

				TR.Add(NewObj);
			}

			return TR;
		}
		#endregion

		#region | DataTable: ToXml |
		/// <summary>
		/// Convert a DataTable to xml string
		/// </summary>
		/// <param name="table">Table to convert</param>
		/// <returns>Eml string</returns>
		public static string ToXml(this DataTable table)
		{
			if ( table == null )
			{
				return null;
			}
			else
			{
				using ( var sw = new StringWriter() )
				using ( var tw = new XmlTextWriter(sw) )
				{
					// Must set name for serialization to succeed
					//table.TableName = @"MyTable";

					// --
					// http://bit.ly/a75DK7

					tw.Formatting = Formatting.Indented;
					tw.WriteStartDocument();
					tw.WriteStartElement(@"data");

					((IXmlSerializable)table).WriteXml(tw);

					tw.WriteEndElement();
					tw.WriteEndDocument();

					tw.Flush();
					tw.Close();
					sw.Flush();

					return sw.ToString();
				}
			}
		}
		#endregion
	}
}
