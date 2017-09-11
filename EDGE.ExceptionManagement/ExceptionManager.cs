using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Xml;

namespace EDGE.ExceptionManagement
{
	#region | ExceptionManager Class definition |
	/// <summary>
	/// The Exception Manager class manages the publishing of exception information based on 
	/// settings in the configuration file, or the default Publisher.
	/// </summary>
	public sealed class ExceptionManager
	{
		#region | Class Constructor |
		/// <summary>
		/// Private constructor to restrict an instance of this class from being created.
		/// </summary>
		private ExceptionManager()
		{
		}
		#endregion

		#region | Member variable declarations |
		private const string EXCEPTIONMANAGEMENT_CONFIG_SECTION = "exceptionManagement";
		private const string RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION = "Information could not be accessed.";
		private const string RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED = "Permission to access this information has been denied.";
		private readonly static string EXCEPTIONMANAGER_NAME = typeof(ExceptionManager).Name;
		#endregion

		#region | Publish method defintion - no additional info |	
		/// <summary>
		/// Static method to publish the exception information.
		/// </summary>
		/// <remarks>Executes the main Publish method, passing in a NULL object for 
		/// additional info</remarks>
		/// <param name="exception">The exception object whose information should be 
		/// published.</param>
		public static Guid Publish(Exception exception)
		{			
			return ExceptionManager.Publish(exception, null);					
		}
		#endregion

		#region | Publish method definition |
		/// <summary>
		/// Static method to publish the exception information and any additional information.
		/// </summary>
		/// <remarks>
		/// This is the method executed by the client when an exception is first caught.
		/// It accepts the exception object, and any additional information (e.g., Machine Name,
		/// UserName, etc.).  If the additional info is empty, this method generates the info.
		/// Then, information contained within the Configuration File is processed.  If a config
		/// file exists, and the publisher nodes have been defined and turned "on", the
		/// ArrayList of the publisher nodes is sent to the BusinessClassPublisherFactory
		/// to generate the PublisherChain for exception handling.  Otherwise, the 
		/// BusinessClassPublisherFactory returns the default CublisherChain.
		/// </remarks>
		/// <param name="exception">The exception object whose information should be 
		/// published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be published
		/// long with the exception information.</param>
		public static Guid Publish(Exception exception, NameValueCollection additionalInfo)
		{
			Guid ExceptionId = Guid.NewGuid();

			try
			{
				#region Create the Additional Information collection if it does not exist.
				if (null == additionalInfo) additionalInfo = new NameValueCollection();

				additionalInfo.Add("ExceptionId", ExceptionId.ToString());
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".MachineName", Environment.MachineName);
				}
				catch(SecurityException)
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".MachineName", RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED);
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".MachineName", RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION);
				}
					
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".TimeStamp", DateTime.Now.ToString());
				}
				catch(SecurityException)
				{					
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".TimeStamp",RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED);
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".TimeStamp", RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION);
				}					
									
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".FullName", Assembly.GetEntryAssembly().FullName);
				}
				catch(SecurityException)
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".FullName", RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED);
				}	
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".FullName", RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION);
				}
					
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AppDomainName", AppDomain.CurrentDomain.FriendlyName);
				}
				catch(SecurityException)
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AppDomainName", RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED);
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AppDomainName", RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION);
				}
						
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ThreadIdentity", Thread.CurrentPrincipal.Identity.Name);
				}
				catch(SecurityException)
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ThreadIdentity", RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED);
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ThreadIdentity", RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION);
				}
				
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".WindowsIdentity", WindowsIdentity.GetCurrent().Name);
				}
				catch(SecurityException)
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".WindowsIdentity", RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED);
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".WindowsIdentity", RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION);
				}

				// Get the Exception Message for insert
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".BriefMessage", exception.InnerException.Message);
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".BriefMessage", exception.Message);
				}

				try
				{
					string location = exception.InnerException.StackTrace.ToString();
					string[] stack = location.Split("\r\n".ToCharArray());
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".LastStack", stack[0]);
				}
				catch
				{
					try
					{
						additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".LastStack", exception.InnerException.StackTrace);
					}
					catch
					{
						//Do Nothing
					}
				}


				#endregion

				Exception expoException = GetExpoExchangeException(exception);
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AssemblyName", expoException.Source);
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AssemblyName", "");
				}

				// Assembly Version and Date
				bool found = false;
				System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
				foreach (System.Reflection.Assembly assembly in assemblies)
				{
					if (string.Compare(assembly.GetName().Name, expoException.Source, true) == 0)
					{
						found = true;
						try
						{
							additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AssemblyVersion", assembly.GetName().Version.ToString());
						}
						catch
						{
							additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AssemblyVersion", "");
						}

						try
						{
							System.IO.FileInfo f = new System.IO.FileInfo(assembly.Location);
							additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AssemblyDate", f.LastWriteTime.ToString());
						}
						catch
						{
							additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AssemblyDate", "");
						}
						break;
					}
				}

				if (!found)
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AssemblyVersion", "");
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".AssemblyDate", "");
				}

				// Server Name
				try
				{
                    // VS2005
                    if (System.Configuration.ConfigurationManager.AppSettings["SqlServer"] != null)
					{
                        additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ServerName", System.Configuration.ConfigurationManager.AppSettings["SqlServer"].ToString());
					}
					else
					{
						additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ServerName", "");
					}
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ServerName", "");
				}

				// Exception Type
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ExceptionType", GetInnerMostException(exception).GetType().FullName);
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ExceptionType", "");
				}
				
				// ExceptionCode
				try
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ExceptionCode", GetInnerMostException(exception).GetHashCode().ToString());
				}
				catch
				{
					additionalInfo.Add(EXCEPTIONMANAGER_NAME + ".ExceptionCode", "");
				}
				
				// Check for any settings in config file.
				ArrayList Publishers = null;

                // VS2005
				if (System.Configuration.ConfigurationManager.GetSection(EXCEPTIONMANAGEMENT_CONFIG_SECTION) != null)
				{
					ExceptionManagementSettings config = (ExceptionManagementSettings)System.Configuration.ConfigurationManager.GetSection(EXCEPTIONMANAGEMENT_CONFIG_SECTION);

					if (config.Mode == ExceptionManagementMode.On)
					{
						if (config.Publishers != null && config.Publishers.Count > 0)
						{					
							Publishers = config.Publishers;
						}
					}
				}

				IPublisher Publisher = BusinessClassPublisherFactory.GetPublisher(Publishers);
				Publisher.Publish(exception, additionalInfo, null);
			}
			catch
			{
				// Publish the original exception and additional information to
				// the default publisher.
				PublishInternalException(exception,additionalInfo);
			}
			return ExceptionId;
		} 
		#endregion

		private static Exception GetInnerMostException(Exception exception)
		{
			if (exception.InnerException != null)
				return GetInnerMostException(exception.InnerException);
			else
				return exception;
		}

		private static Exception GetExpoExchangeException(Exception exception)
		{
            // MANDLERR - Added null check on Source property.  I think this was causing something like
            // ExceptionManager.Publish(new Exception("This is an exception")) to fail to send the email
            // through the EmailPublisher.
			if (exception.Source != null && exception.Source.Substring(0, 4).ToLower() == "expo")
				return exception;
			else if (exception.InnerException != null)
				return GetExpoExchangeException(exception.InnerException);
			else
				return exception;
		}

		#region | PublishInternalException method definition |
		/// <summary>
		/// Private static helper method to publish the exception information to the default 
		/// publisher.
		/// </summary>
		/// <remarks>
		/// This simply executes the PublisherChain with the default values.  It is called from  
		/// the "catch" within the ExceptionManager Publish method.
		/// </remarks>
		/// <param name="exception">The exception object whose information should be 
		/// published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be 
		/// published along with the exception information.</param>
		internal static void PublishInternalException(Exception exception, NameValueCollection additionalInfo)
		{
			// Get the Default Publisher
			IPublisher defaultPublisher = BusinessClassPublisherFactory.GetPublisher(null);
			defaultPublisher.Publish(exception, additionalInfo,null);
		}
		#endregion

		#region | SerializeToXML method definition  |
		/// <summary>
		/// Public static helper method to serialize the exception information into XML.
		/// </summary>
		/// <remarks>
		/// The node names in the XML document produced are controlled by variables within 
		/// this class.  
		/// </remarks>
		/// <param name="exception">The exception object whose information should be 
		/// published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be 
		/// published along with the exception information.</param>
		public static XmlDocument SerializeToXml(Exception exception, NameValueCollection additionalInfo)
		{
			try
			{
				// Variable declarations
				string ROOT = "ExceptionInformation";
				string ADDITIONAL_INFORMATION = "AdditionalInformationProperty";
				string EXCEPTION = "Exception";
				string STACK_TRACE = "StackTrace";
                string DATA = "Data";
				XmlElement element;
				XmlElement exceptionAddInfoElement;
				XmlElement stackTraceElement;
                XmlElement dataElement;
				XmlText stackTraceText;
				XmlAttribute attribute;
			
				// Create a new XmlDocument.
				XmlDocument xmlDoc = new XmlDocument();

				// Create the root node.
				XmlElement root = xmlDoc.CreateElement(ROOT);
				xmlDoc.AppendChild(root);


				// Check if the collection has values.
				if (additionalInfo != null && additionalInfo.Count > 0)
				{
					// Create the element for the collection.
					element = xmlDoc.CreateElement(ADDITIONAL_INFORMATION);
				
					// Loop through the collection and add the values as attributes on the element.
					foreach (string i in additionalInfo)
					{
						attribute = xmlDoc.CreateAttribute(i.Replace(" ", "_"));
						attribute.Value = additionalInfo.Get(i);
						element.Attributes.Append(attribute);
					}

					// Add the element to the root.
					root.AppendChild(element);
				}

				if (exception == null)
				{
					// Create an empty exception element.
					element = xmlDoc.CreateElement(EXCEPTION);

					// Append to the root node.
					root.AppendChild(element);
				}
				else
				{
					// Loop through each exception class in the chain of exception objects.
					Exception currentException = exception;	
					XmlElement parentElement = null;	
					do
					{
						// Create the exception element.
						element = xmlDoc.CreateElement(EXCEPTION);

						// Add the exceptionType as an attribute.
						attribute = xmlDoc.CreateAttribute("ExceptionType");
						attribute.Value = currentException.GetType().FullName;
						element.Attributes.Append(attribute);
				
						// Loop through the public properties of the exception object and record their value.
						PropertyInfo[] aryPublicProperties = currentException.GetType().GetProperties();
						NameValueCollection currentAdditionalInfo;
						foreach (PropertyInfo p in aryPublicProperties)
						{
							// Do not log information for the InnerException, StackTrace or Data. This information is 
							// captured later in the process.
							if (p.Name != "InnerException" && p.Name != "StackTrace" && p.Name != "Data")
							{
								// Only record properties whose value is not null.
								if (p.GetValue(currentException,null) != null)
								{
									// Check if the property is AdditionalInformation and the exception type is a BaseApplicationException.
									if (p.Name == "AdditionalInformation" && currentException is BaseApplicationException)
									{
										// Verify the collection is not null.
										if (p.GetValue(currentException,null) != null)
										{
											// Cast the collection into a local variable.
											currentAdditionalInfo = (NameValueCollection)p.GetValue(currentException,null);

											// Verify the collection has values.
											if (currentAdditionalInfo.Count > 0)
											{
												// Create element.
												exceptionAddInfoElement = xmlDoc.CreateElement(ADDITIONAL_INFORMATION);

												// Loop through the collection and add values as attributes.
												foreach (string i in currentAdditionalInfo)
												{
													attribute = xmlDoc.CreateAttribute(i.Replace(" ", "_"));
													attribute.Value = currentAdditionalInfo.Get(i);
													exceptionAddInfoElement.Attributes.Append(attribute);
												}

												element.AppendChild(exceptionAddInfoElement);
											}
										}
									}
										// Otherwise just add the ToString() value of the property as an attribute.
									else
									{
										attribute = xmlDoc.CreateAttribute(p.Name);
										attribute.Value = p.GetValue(currentException,null).ToString();
										element.Attributes.Append(attribute);
									}
								}
							}
						}

                        // Record the Data within a separate element
                        if ( currentException.Data != null && currentException.Data.Count > 0 )
                        {
                            dataElement = xmlDoc.CreateElement(DATA);
                            foreach(var key in currentException.Data.Keys )
                            {
                                var val = currentException.Data[key];
                                if ( val != null )
                                    dataElement.SetAttribute(key.ToString(), val.ToString());
                            }
                            element.AppendChild(dataElement);
                        }

						// Record the StackTrace within a separate element.
						if (currentException.StackTrace != null)
						{
							// Create Stack Trace Element.
							stackTraceElement = xmlDoc.CreateElement(STACK_TRACE);

							stackTraceText = xmlDoc.CreateTextNode(currentException.StackTrace.ToString());

							stackTraceElement.AppendChild(stackTraceText);

							element.AppendChild(stackTraceElement);
						}

						// Check if this is the first exception in the chain.
						if (parentElement == null)
						{
							// Append to the root node.
							root.AppendChild(element);
						}
						else
						{
							// Append to the parent exception object in the exception chain.
							parentElement.AppendChild(element);
						}
				
						// Reset the temp variables.
						parentElement = element;
						currentException = currentException.InnerException;

						// Continue looping until we reach the end of the exception chain.
					} while (currentException != null);
				}
				// Return the XmlDocument.
				return xmlDoc;
			}
			catch(Exception e)
			{
				throw new SerializationException("Exception Manager could not serialize the Exception information into XML.",e);
			}
		}
		#endregion
	}
	#endregion
}   