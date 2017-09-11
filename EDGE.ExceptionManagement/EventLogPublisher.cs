using System;
using System.Reflection;
using System.Collections;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Collections.Specialized;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;

namespace EDGE.ExceptionManagement
{
	#region | EventLogPublisher class definition |
	/// <summary>
	/// The implementation of the BasePublisher class which writes the
	/// Exception to the application event log.
	/// </summary>
	public class EventLogPublisher : BasePublisher
	{
		#region | Member variable declarations |
		/// <summary>
		/// Hard-code defaults in case no conifg files exist.
		/// </summary>
		private string _LogName = "Application";
		private string _ApplicationName = "EDGE.ExceptionManagement";	//resourceManager.GetString("RES_EXCEPTIONMANAGER_PUBLISHED_EXCEPTIONS");
		private const string TEXT_SEPARATOR = "*********************************************";
		#endregion

		#region | Class Constructors |
		/// <summary>
		/// Empty constructor for the EventLogPublisher class.
		/// </summary>
		public EventLogPublisher()
		{
		}
		#endregion
		
		#region | Publish method declaration |
		/// <summary>
		/// Method used to publish exception information to the Application Event Log.
		/// </summary>
		/// <remarks>If the method fails, the next Publisher in the PublisherChain is executed.</remarks>
		/// <param name="exception">The exception object whose information should be published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
		/// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
		public override void Publish(Exception exception, NameValueCollection additionalInfo,NameValueCollection configSettings)
		{
			// Load Config values if they are provided.
			configSettings = this.ConfigSettings;
			if (configSettings != null)
			{
				if (configSettings["applicationName"] != null && configSettings["applicationName"].Length > 0) _ApplicationName = configSettings["applicationName"];
				if (configSettings["logName"] != null && configSettings["logName"].Length > 0)  _LogName = configSettings["logName"];
			}

			try
			{
				// Verify that the Source exists before gathering exception information.
				VerifyValidSource();

				// Create StringBuilder to maintain publishing information.
				StringBuilder PublishingInfo = new StringBuilder();

				// Record the contents of the AdditionalInfo collection.
				if (additionalInfo != null)
				{
					// Record General information.
					PublishingInfo.AppendFormat("{0}General Information {0}{1}{0}Additional Info:", Environment.NewLine, TEXT_SEPARATOR);

					foreach (string InfoItem in additionalInfo)
					{
						PublishingInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, InfoItem, additionalInfo.Get(InfoItem));
					}
				}

				if (exception == null)
				{
					PublishingInfo.AppendFormat("{0}{0}No Exception object has been provided.{0}", Environment.NewLine);
				}
				else
				{
					// Loop through each exception class in the chain of exception objects.
					Exception CurrentException = exception;	
					int ExceptionCount = 1;				
					do
					{
						// Write title information for the exception object.
						PublishingInfo.AppendFormat("{0}{0}{1}) Exception Information{0}{2}", Environment.NewLine, ExceptionCount.ToString(), TEXT_SEPARATOR);
						PublishingInfo.AppendFormat("{0}Exception Type: {1}", Environment.NewLine, CurrentException.GetType().FullName);
				
						// Loop through the public properties of the exception object and record their value.
						PropertyInfo[] aryPublicProperties = CurrentException.GetType().GetProperties();
						NameValueCollection CurrentAdditionalInfo;
						foreach (PropertyInfo p in aryPublicProperties)
						{
							// Do not log information for the InnerException or StackTrace. This information is 
							// captured later in the process.
							if (p.Name != "InnerException" && p.Name != "StackTrace")
							{
								if (p.GetValue(CurrentException,null) == null)
								{
									PublishingInfo.AppendFormat("{0}{1}: NULL", Environment.NewLine, p.Name);
								}
								else
								{
									// Loop through the collection of AdditionalInformation if the exception type is a BaseApplicationException.
									if (p.Name == "AdditionalInformation" && CurrentException is BaseApplicationException)
									{
										// Verify the collection is not null.
										if (p.GetValue(CurrentException,null) != null)
										{
											// Cast the collection into a local variable.
											CurrentAdditionalInfo = (NameValueCollection)p.GetValue(CurrentException,null);

											// Check if the collection contains values.
											if (CurrentAdditionalInfo.Count > 0)
											{
												PublishingInfo.AppendFormat("{0}AdditionalInformation:", Environment.NewLine);

												// Loop through the collection adding the information to the string builder.
												for (int i = 0; i < CurrentAdditionalInfo.Count; i++)
												{
													PublishingInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, CurrentAdditionalInfo.GetKey(i), CurrentAdditionalInfo[i]);
												}
											}
										}
									}
										// Otherwise just write the ToString() value of the property.
									else
									{
										PublishingInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, p.Name, p.GetValue(CurrentException,null));
									}
								}
							}
						}
						// Record the StackTrace with separate label.
						if (CurrentException.StackTrace != null)
						{
							PublishingInfo.AppendFormat("{0}{0}StackTrace Information{0}{1}", Environment.NewLine, TEXT_SEPARATOR);
							PublishingInfo.AppendFormat("{0}{1}", Environment.NewLine, CurrentException.StackTrace);
						}

						// Reset the temp exception object and iterate the counter.
						CurrentException = CurrentException.InnerException;
						ExceptionCount++;
					} while (CurrentException != null);
				}

				// Write the entry to the event log.   
				WriteToLog(PublishingInfo.ToString(), EventLogEntryType.Error);
			}
			catch
			{
			}
			finally
			{
				if (base.NextPublisher != null)
					base.NextPublisher.Publish( exception, additionalInfo, null);
			}
		}
		#endregion

		#region | Helper function definitions |
		/// <summary>
		/// Helper function to write an entry to the Event Log.
		/// </summary>
		/// <param name="entry">The entry to enter into the Event Log.</param>
		/// <param name="type">The EventLogEntryType to be used when the entry is logged to the Event Log.</param>
		/// <exception cref="SecurityException">Thrown when the event source does not exist.</exception>
		private void WriteToLog(string entry, EventLogEntryType type)
		{
			try
			{
				// Write the entry to the Event Log.
				EventLog.WriteEntry(_ApplicationName,entry,type);
			}
			catch(SecurityException e)
			{				
				throw new SecurityException(String.Format("The event source {0} does not exist and cannot be created with the current permissions.", _ApplicationName),e);
			}
		}

		/// <summary>
		/// Helper function to ensure the eventlog source exists.
		/// </summary>
		/// <exception cref="SecurityException">Thrown when the event source does not exist.</exception>
		private void VerifyValidSource()
		{
			try
			{
				if (!EventLog.SourceExists(_ApplicationName))
				{
					EventLog.CreateEventSource(_ApplicationName, _LogName);
				}
			}
			catch(SecurityException e)
			{
				throw new SecurityException(String.Format("The event source {0} does not exist and cannot be created with the current permissions.", _ApplicationName),e);
			}
		}
		#endregion
	}
	#endregion
}