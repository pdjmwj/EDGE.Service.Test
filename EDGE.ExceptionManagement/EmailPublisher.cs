using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;

namespace EDGE.ExceptionManagement
{
	#region | EmailPublisher class definition |
	/// <summary>
	/// The implementation of the BasePublisher class which sends the
	/// Exception to a specified person via SMTP email.
	/// </summary>
	public class EmailPublisher : BasePublisher
	{
		#region | Member variable declarations |
		//Hard-code defaults in case no conifg files exist.
		private string _OpMail = "RegNetEmailAlertGroup@expoexchange.com";
		private string _FromMail = "RegNetError@ExpoExchange.com";
		private string _Subject = "Exception Notification";
		private string _SMTPServer = "";
		#endregion

		#region | Class Constructors |
		/// <summary>
		/// Empty constructor for the SQLServerPublisher class.
		/// </summary>
		public EmailPublisher()
		{
		}
		#endregion

		#region | Publish method declaration |
		/// <summary>
		/// Method used to publish exception information via an email.
		/// </summary>
		/// <remarks>
		/// This particular method sends the Exception in an email to a designated
		/// recipient. By default, the class uses the local server as the SMTP Server.
		/// However, the SMTP Server, as well as the to email, from email and subject
		/// can be changed in the class or in the config file by changing the appropriate
		/// variables or parameters.
		/// If the method fails, the next Publisher in the PublisherChain is executed.</remarks>
		/// <param name="exception">The exception object whose information should be published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
		/// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
		public override void Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings)
		{


			// Load Config values if they are provided.
			configSettings = this.ConfigSettings;
			if (configSettings != null)
			{

				if (configSettings["operatorMail"] !=null && 
					configSettings["operatorMail"].Length > 0)
				{
					_OpMail = configSettings["operatorMail"];

                    if (!string.IsNullOrEmpty(_OpMail))
                    {
                        // JDM (2009.11.12): replace semicolon with comma:
                        // http://msdn.microsoft.com/en-us/library/ms144695(VS.80).aspx
                        _OpMail = _OpMail.Replace(";", ",").Trim();
                        // JDM (2009.11.18): if ends with comma, strip off to avoid format exception
                        if (_OpMail.EndsWith(","))
                            _OpMail = _OpMail.Substring(0, _OpMail.Length - 1);
                    }
				}
				
				if (configSettings["fromMail"] !=null && 
					configSettings["fromMail"].Length > 0)
				{
					_FromMail = configSettings["fromMail"];
				}

				if (configSettings["subject"] !=null && 
					configSettings["subject"].Length > 0)
				{
					_Subject = configSettings["subject"];
				}

				if (configSettings["SMTPServer"] !=null && 
					configSettings["SMTPServer"].Length > 0)
				{
					_SMTPServer = configSettings["SMTPServer"];
				}
			}

			try
			{
				// Generate the Body of the email
				StringBuilder ExceptionInfo = new StringBuilder();

				if (additionalInfo != null)
				{
					ExceptionInfo.AppendFormat("{0}General Information{0}", Environment.NewLine);
					ExceptionInfo.AppendFormat("{0}Additonal Info:", Environment.NewLine);
					foreach (string EachInfo in additionalInfo)
					{
						ExceptionInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, EachInfo, additionalInfo.Get(EachInfo));
					}
				}
				ExceptionInfo.AppendFormat("{0}{0}Exception Information{0}{1}", Environment.NewLine, exception.ToString());

				// Send notification email 
				string Body = ExceptionInfo.ToString();
                // VS2005
                System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient(_SMTPServer);
                client.Send(_FromMail, _OpMail, _Subject, Body);
			}
			catch
			{
			}
			finally
			{
				if (base.NextPublisher != null)
					base.NextPublisher.Publish( exception,  additionalInfo,  configSettings);
			}
		}
		#endregion
	}
	#endregion
}