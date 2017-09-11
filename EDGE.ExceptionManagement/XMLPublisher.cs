using System;
using System.Text;
using System.IO;
using System.Xml;	
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Specialized;

namespace EDGE.ExceptionManagement
{
	#region | XMLPublisher class definition |
	/// <summary>
	/// The implementation of the BasePublisher class which writes the
	/// Exception to an XML file.
	/// </summary>
	public class XMLPublisher : BasePublisher 
	{
		#region | Member variable declarations |
        //Hard-code defaults in case no conifg files exist.
		private string _FileName = @"C:\ErrorLog.xml";
		private bool _InsertTimeStamp = true;

		#endregion

		#region | Class Constructors |
		/// <summary>
		/// Empty constructor for the XMLPublisher class.
		/// </summary>
		public XMLPublisher()
		{
		}
		#endregion

		#region | Publish method declaration |
		/// <summary>
		/// Method used to publish exception information to an XML File.
		/// </summary>
		/// <remarks>This particular method generates a XML file and writes it to the local drive.
		/// By default, the file generated is saved to the root directory (C drive) and
		/// multiple files are created.  The existence of multiple file creation is handled by
		/// adding a DateTime stamp to the FileName.  This can be turned off in the class or in 
		/// the config file by setting the _InsertTimeStamp variable or AddTimeStamp parameter
		/// to false.
		/// If the method fails, the next Publisher in the PublisherChain is executed.</remarks>
		/// <param name="exception">The exception object whose information should be published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
		/// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
		public override void Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings)
		{
			configSettings = this.ConfigSettings;
			if (configSettings != null)
			{
				if (configSettings["fileName"] !=null && 
					configSettings["fileName"].Length > 0)
				{
					_FileName = configSettings["fileName"];
				}

				if (configSettings["AddTimeStamp"] !=null && 
					configSettings["AddTimeStamp"].Length > 0)
				{
					_InsertTimeStamp = Convert.ToBoolean(configSettings["AddTimeStamp"]);
				}
			}

			StreamWriter Writer = null;
			try 
			{
				// Wrap the exception into an XML document
				XmlDocument ExceptionInfo = ExceptionManager.SerializeToXml(exception,additionalInfo);

				// Insert datetime stamp into Error Log FileName
				if (_InsertTimeStamp)
				{
					DateTime CurrentDateTime = System.DateTime.Now;
					string DateTimeStamp = "_" + CurrentDateTime.Year.ToString() + "_" + CurrentDateTime.Month.ToString().PadLeft(2,'0') + "_" + CurrentDateTime.Day.ToString().PadLeft(2,'0') + "_" + CurrentDateTime.Hour.ToString().PadLeft(2,'0') + "_" + CurrentDateTime.Minute.ToString().PadLeft(2,'0') + "_" + CurrentDateTime.Second.ToString().PadLeft(2,'0');
					int DotLocation = _FileName.LastIndexOf(".");
					_FileName = _FileName.Insert(DotLocation,DateTimeStamp);
				}

				// Write the XML content to the file.
				using ( FileStream fs =  File.Open(_FileName, FileMode.Append, FileAccess.Write) )
				{
					using ( Writer = new StreamWriter(fs) )
					{
						Writer.WriteLine(ExceptionInfo.OuterXml);
						Writer.WriteLine();
						Writer.WriteLine();
					}
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				// in case of an error
			}
			finally
			{
				if(Writer!=null)
					Writer.Close();
				if (base.NextPublisher != null)
					base.NextPublisher.Publish( exception,  additionalInfo,  configSettings);		
			}
		}
		#endregion

	}
	#endregion
}
