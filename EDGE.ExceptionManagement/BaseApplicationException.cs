using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Security;
using System.Security.Principal;
using System.Security.Permissions;
using System.Collections.Specialized;

namespace EDGE.ExceptionManagement
{
	#region | BaseApplicationException |
	/// <summary>
	/// Base Application Exception Class. You can use this as the base exception object from
	/// which to derive your applications exception hierarchy.
	/// </summary>
	[Serializable]
	public class BaseApplicationException : ApplicationException
	{
		#region | Constructors |
		/// <summary>
		/// Constructor with no params.
		/// </summary>
		public BaseApplicationException() : base()
		{
			InitializeEnvironmentInformation();
		}
		/// <summary>
		/// Constructor allowing the Message property to be set.
		/// </summary>
		/// <param name="message">String setting the message of the exception.</param>
		public BaseApplicationException(string message) : base(message) 
		{
			InitializeEnvironmentInformation();
		}
		/// <summary>
		/// Constructor allowing the Message and InnerException property to be set.
		/// </summary>
		/// <param name="message">String setting the message of the exception.</param>
		/// <param name="inner">Sets a reference to the InnerException.</param>
		public BaseApplicationException(string message,Exception inner) : base(message, inner)
		{
			InitializeEnvironmentInformation();
		}
		/// <summary>
		/// Constructor used for deserialization of the exception class.
		/// </summary>
		/// <param name="info">Represents the SerializationInfo of the exception.</param>
		/// <param name="context">Represents the context information of the exception.</param>
		protected BaseApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			_MachineName = info.GetString("machineName");
			_CreatedDateTime = info.GetDateTime("createdDateTime");
			_AppDomainName = info.GetString("appDomainName");
			_ThreadIdentity = info.GetString("threadIdentity");
			_WindowsIdentity = info.GetString("windowsIdentity");
			_AdditionalInformation = (NameValueCollection)info.GetValue("additionalInformation",typeof(NameValueCollection));
		}
		#endregion

		#region | Member Variables declarations |
		private string _MachineName; 
		private string _AppDomainName;
		private string _ThreadIdentity; 
		private string _WindowsIdentity; 
		private DateTime _CreatedDateTime = DateTime.Now;
		private const string RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION = "Information could not be accessed.";
		private const string RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED = "Permission to access this information has been denied.";
		private NameValueCollection _AdditionalInformation = new NameValueCollection();	
		#endregion

		#region | GetObjectData method definition |
		/// <summary>
		/// Override the GetObjectData method to serialize custom values.
		/// </summary>
		/// <param name="info">Represents the SerializationInfo of the exception.</param>
		/// <param name="context">Represents the context information of the exception.</param>
		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData( SerializationInfo info, StreamingContext context ) 
		{
			info.AddValue("machineName", _MachineName, typeof(string));
			info.AddValue("createdDateTime", _CreatedDateTime);
			info.AddValue("appDomainName", _AppDomainName, typeof(string));
			info.AddValue("threadIdentity", _ThreadIdentity, typeof(string));
			info.AddValue("windowsIdentity", _WindowsIdentity, typeof(string));
			info.AddValue("additionalInformation", _AdditionalInformation, typeof(NameValueCollection));
			base.GetObjectData(info,context);
		}
		#endregion

		#region | Public Properties |
		/// <summary>
		/// Machine name where the exception occurred.
		/// </summary>
		public string MachineName
		{
			get
			{
				return _MachineName;
			}
		}

		/// <summary>
		/// Date and Time the exception was created.
		/// </summary>
		public DateTime CreatedDateTime
		{
			get
			{
				return _CreatedDateTime;
			}
		}

		/// <summary>
		/// AppDomain name where the exception occurred.
		/// </summary>
		public string AppDomainName
		{
			get
			{
				return _AppDomainName;
			}
		}

		/// <summary>
		/// Identity of the executing thread on which the exception was created.
		/// </summary>
		public string ThreadIdentityName
		{
			get
			{
				return _ThreadIdentity;
			}
		}

		/// <summary>
		/// Windows identity under which the code was running.
		/// </summary>
		public string WindowsIdentityName
		{
			get
			{
				return _WindowsIdentity;
			}
		}

		/// <summary>
		/// Collection allowing additional information to be added to the exception.
		/// </summary>
		public NameValueCollection AdditionalInformation
		{
			get
			{
				return _AdditionalInformation;
			}
		}
		#endregion

		#region | InitializeEnvironmentInformation method definition |
		/// <summary>
		/// Initialization function that gathers environment information safely.
		/// </summary>
		/// <remarks>This routine attempts to retrieve environemnt information, 
		/// such as Machine Name, Application, Thread Name, and place the data into
		/// the private variables for the class.  These variables are then added to
		/// the AdditionalInfo collection for later use.</remarks>
		private void InitializeEnvironmentInformation()
		{									
			try
			{				
				_MachineName = Environment.MachineName;
			}
			catch(SecurityException)
			{
				_MachineName = RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED;
				
			}
			catch
			{
				_MachineName = RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION;
			}
					
			try
			{
				_ThreadIdentity = Thread.CurrentPrincipal.Identity.Name;
			}
			catch(SecurityException)
			{
				_ThreadIdentity = RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED;
			}
			catch
			{
				_ThreadIdentity = RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION;
			}			
			
			try
			{
				_WindowsIdentity = WindowsIdentity.GetCurrent().Name;
			}
			catch(SecurityException)
			{
				_WindowsIdentity = RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED;
			}
			catch
			{
				_WindowsIdentity = RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION;
			}
			
			try
			{					
				_AppDomainName = AppDomain.CurrentDomain.FriendlyName;
			}
			catch(SecurityException)
			{
				_AppDomainName = RES_EXCEPTIONMANAGEMENT_PERMISSION_DENIED;
			}
			catch
			{
				_AppDomainName = RES_EXCEPTIONMANAGEMENT_INFOACCESS_EXCEPTION;
			}			
		}
		#endregion
	}	
	#endregion
}