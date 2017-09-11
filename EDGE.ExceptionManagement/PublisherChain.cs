using System;
using System.Collections.Specialized;
using System.Collections;
using System.Xml;

namespace EDGE.ExceptionManagement
{
	#region | Definition of IPublisher interface |
	/// <summary>
	/// Defines the base interface for each publisher to inherit from.
	/// Contains a single method, Publish.
	/// </summary>
	public interface IPublisher
	{
		/// <summary>
		/// Method used to publish exception information and additional information.
		/// </summary>
		/// <param name="exception">The exception object whose information should be published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
		/// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
		void Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings);
	}
	#endregion
	
	#region | Definition of IPublisherChain interface |
	/// <summary>
	/// Defines base interface for the PublisherChain object.
	/// This ojbect is a linked-list, which is simply an an object
	/// which holds links of nested objects.
	/// Contains two methods, NextPublisher and Tail.
	/// </summary>
	public interface IPublisherChain : IPublisher
	{
		/// <summary>
		/// Holds a reference to the next "link" in the object chain.
		/// </summary>
		IPublisherChain NextPublisher
		{
			get;
			set;
		}

		/// <summary>
		/// Read-only property which returns the last link in the Chain.
		/// </summary>
		IPublisherChain Tail
		{	
			get;
		}
	}
	#endregion

	#region | Definition of BasePublisher class |
	/// <summary>
	/// The base class implementation of the PublisherChain interface.</summary>
	/// <remarks>
	/// In this implementation, it is holding references to the different
	/// types of Publisher objects (ie, SQLServerPublisher, XMLPublisher, EmailPublisher,
	/// or EventLogPublisher).  The BasePublisher holds the reference to these objects
	/// and maintains the order of execution of the exception handling.
	/// </remarks>
	public abstract class BasePublisher : IPublisherChain
	{
		#region | Member variable declarations |
		/// <summary>
		/// Class member variables.
		/// </summary>
		private IPublisherChain _NextPublisher;
		private NameValueCollection _ConfigSettings;
		#endregion

		#region | Publish method declaration |
		/// <summary>
		/// Method used to publish exception information and additional information.
		/// used as a "place holder" for the other inherited Publisher classes.
		/// </summary>
		/// <param name="exception">The exception object whose information should be published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
		/// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
		public abstract void Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings);
		#endregion

		#region | + operator Defition |
		/// <summary>
		/// Appends the designated BasePublisher object (p2) to the end of the current chain (p1).
		/// </summary>
		/// <remarks>
		/// Actually, it is setting the p2 object as the NextPublisher property of the current Tail
		/// object in the Chain.
		///</remarks>+
		/// <param name="p1">The (relative) main BasePublisher object.  This is the one which is being appended to.</param>
		/// <param name="p2">The (relative) next BasePublisher object.  This is the one which is being added as the NextPublisher object.</param>
		/// <returns>The appended BasePublisher object, or null</returns>
		public static BasePublisher operator +(BasePublisher p1, BasePublisher p2) 
		{
			if((p1!=null)&&(p2!=null))
			{
				p1.Tail.NextPublisher = p2;
				return p1;
			}

			if(p1!=null)
				return p1;

			if(p2!=null)
				return p2;

			return null;
		}
		#endregion
		
		#region | NextPublisher property definition |
		/// <summary>
		/// Holds the reference to (each) next object in the chain.
		/// </summary>
		public IPublisherChain NextPublisher
		{
			get
			{
				return this._NextPublisher;
			}
			set
			{
				this._NextPublisher = value;
			}
		}
		#endregion

		#region | Tail property definition |
		/// <summary>
		/// Read-only.  Returns the last link (Publisher object) in the chain.
		/// </summary>
		public IPublisherChain Tail
		{
			get
			{
				if(this.NextPublisher!=null)
					return this.NextPublisher.Tail;
				else
					return (IPublisherChain)this;
			}
		}
		#endregion

		#region | ConfigSettings property definition |
		/// <summary>
		/// Holds the configuration information for each Publisher object.
		/// </summary>
		/// <remarks>This information is used within each Implementation of the BasePublisher
		/// to allow the client to override defaults via the config file.
		/// </remarks>
		public NameValueCollection ConfigSettings
		{
			get
			{
				return	_ConfigSettings;
			}
			set
			{
				_ConfigSettings = value;
			}
		}
		#endregion

	}
	#endregion

	#region | Definition of BusinessClassPublisherFactory class |
	/// <summary>
	/// This class is what is executed from the ExceptionManager to 
	/// generate the PublisherChain object.
	/// </summary>
	public class BusinessClassPublisherFactory
	{
		#region | Class Constructors |
		/// <summary>
		/// Empty constructor for the BusinessClassPublisherFactory class.
		/// </summary>
		private BusinessClassPublisherFactory()
		{
		}
		#endregion

		#region | GetPublisher method declaration |
		/// <summary>
		/// Returns the PublisherChain for exception handling
		/// </summary>
		/// <remarks>
		/// This is the method called from ExceptionManager and bulds the publisher chain.  
		/// It accepts an arraylist of the PublisherSettings, as defined in the configuration file.  
		/// This defines which publishers the client wants to use, and the order in which to execute
		/// each one. If no settings are passed, the default order of exception publishing is returned.
		/// The current order of publishing is:
		/// <list type="bullet">
		/// <item><description>SQLServerPublisher</description></item>
		/// <item><description>XMLPublisher</description></item>
		/// <item><description>EmailPublisher</description></item>
		/// <item><description>EventLogPublisher</description></item>
		/// </list>
		/// </remarks>
		/// <example>This sample shows how the default PublisherChain can be returned.
		/// <code>
		///	IPublisher defaultPublisher = BusinessClassPublisherFactory.GetPublisher(null);
		/// defaultPublisher.Publish(exception, additionalInfo,null);
		/// </code>
		/// </example>
		/// <param name="publishers">An arraylist of the publisher nodes within the configuration file</param>
		/// <returns>A PublisherChain object containing all references to the Publisher objects in the desired order of execution.</returns>
		public static IPublisher GetPublisher(ArrayList publishers)
		{
			BasePublisher TempPublisher = new SQLServerPublisher();
			bool IsFirst = true;

			// Always load the DEFAULT
			BasePublisher PublisherToReturn = new SQLServerPublisher();
			PublisherToReturn += new XMLPublisher();
			PublisherToReturn += new EmailPublisher();
			PublisherToReturn += new EventLogPublisher();
		
			if (publishers != null)
			{
				foreach(PublisherSettings PubSettings in publishers)
				{
					if (PubSettings.Mode == PublisherMode.On)
					{
						if (IsFirst)
						{
							PublisherToReturn = (BasePublisher)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(PubSettings.AssemblyName ,PubSettings.TypeName);
							PublisherToReturn.ConfigSettings = PubSettings.OtherAttributes;
							IsFirst = false;
						}
						else
						{
							TempPublisher = (BasePublisher)AppDomain.CurrentDomain.CreateInstanceAndUnwrap(PubSettings.AssemblyName ,PubSettings.TypeName);
							TempPublisher.ConfigSettings = PubSettings.OtherAttributes;
							PublisherToReturn += TempPublisher;
						}
					}
				}
			}

			return (IPublisher)PublisherToReturn;
		}
		#endregion
	}
	#endregion
}