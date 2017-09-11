using System;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;	
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace EDGE.ExceptionManagement
{
	/// <summary>
	/// The implementation of the BasePublisher class which writes the
	/// Exception to the EXCEPTION_LOG table of a SQL Server database.
	/// </summary>
	public class SQLServerPublisher : BasePublisher
	{
		#region | Member variable declarations |
		//Hard-code defaults in case no config files exist.
		private string _DBConnString = "";	// EventXL.Common.Business.ConnectionManagerBase.GetWinAuthConnectionString("FREXLDEVSQL01","Enterprise");

		#endregion

		#region | Class Constructors |
		/// <summary>
		/// Empty constructor for the SQLServerPublisher class.
		/// </summary>
		public SQLServerPublisher()
		{
		}
		#endregion

		#region | Publish method declaration |
		/// <summary>
		/// Method used to publish exception information to SQL Server.
		/// </summary>
		/// <remarks> This particular method saves the exception, including the XML text, to a database.
		/// By default, the file generated is stored in the local database.  This can be 
		/// changed in the class or in the config file by changing the _DBConnString variable 
		/// or "dbconn" parameter. The procedure executes the sql_INSERT_EXCEPTION Stored Procedure.
		/// If the method fails, the next Publisher in the PublisherChain is executed. [6/20/2007] 
		/// Change to use connection manager instead of dbconn.</remarks>
		/// <param name="exception">The exception object whose information should be published.</param>
		/// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
		/// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
		public override void Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings)
		{
			string showCode = "";
            // VS2005
            if (System.Configuration.ConfigurationManager.AppSettings["ShowCode"] != null)
			{
                showCode = System.Configuration.ConfigurationManager.AppSettings["ShowCode"].ToString();
				_DBConnString = "";	// new EventXL.Common.Business.ConnectionManagerBase().GetConnectionString(showCode);
			}
			else
			{
				_DBConnString = ""; // new EventXL.Common.Business.ConnectionManagerBase().GetEntConnectionString();
			}

			try
			{
				XmlDocument ExceptionInfo = ExceptionManager.SerializeToXml(exception,additionalInfo);

				SqlConnection DatabaseConnection = new SqlConnection(_DBConnString);
				DatabaseConnection.Open();
				SqlCommand InsertCommand = new SqlCommand("spExceptionLogInsert", DatabaseConnection);

				InsertCommand.CommandType = CommandType.StoredProcedure;
				InsertCommand.Parameters.Add(new SqlParameter("@ExceptionLogID",SqlDbType.Int,0,
					ParameterDirection.Output, false, 0, 0, null, DataRowVersion.Default, 0));
				InsertCommand.Parameters.Add(new SqlParameter("@AppName", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.AppDomainName")));
				InsertCommand.Parameters.Add(new SqlParameter("@UserName", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.WindowsIdentity")));
				InsertCommand.Parameters.Add(new SqlParameter("@MachineName", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.MachineName")));
				InsertCommand.Parameters.Add(new SqlParameter("@OccurredAt", SqlDbType.DateTime, 0,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.TimeStamp")));
				InsertCommand.Parameters.Add(new SqlParameter("@FullAppName", SqlDbType.VarChar, 255,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.FullName")));
				InsertCommand.Parameters.Add(new SqlParameter("@BriefMessage", SqlDbType.NVarChar, 255,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.BriefMessage")));
				InsertCommand.Parameters.Add(new SqlParameter("@ThreadName", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.ThreadIdentity")));
				InsertCommand.Parameters.Add(new SqlParameter("@ErrorLocation", SqlDbType.VarChar, 500,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.LastStack")));
				InsertCommand.Parameters.Add(new SqlParameter("@FullError", SqlDbType.Xml, 0,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, ExceptionInfo.OuterXml));

				InsertCommand.Parameters.Add(new SqlParameter("@AssemblyName", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.AssemblyName")));
				InsertCommand.Parameters.Add(new SqlParameter("@AssemblyVersion", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.AssemblyVersion")));
				InsertCommand.Parameters.Add(new SqlParameter("@AssemblyDate", SqlDbType.DateTime, 0,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.AssemblyDate")));

				InsertCommand.Parameters.Add(new SqlParameter("@ShowCode", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, showCode));
				InsertCommand.Parameters.Add(new SqlParameter("@ServerName", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.ServerName")));

				InsertCommand.Parameters.Add(new SqlParameter("@InnerMostExceptionType", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.ExceptionType")));
				InsertCommand.Parameters.Add(new SqlParameter("@InnerMostExceptionCode", SqlDbType.VarChar, 50,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionManager.ExceptionCode")));

				InsertCommand.Parameters.Add(new SqlParameter("@GUID", SqlDbType.VarChar, 100,
					ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, additionalInfo.Get("ExceptionID")));

				InsertCommand.ExecuteNonQuery();

				DatabaseConnection.Close();
				InsertCommand.Dispose();
				DatabaseConnection.Dispose();

			}
			catch {}
			finally
			{
				if (base.NextPublisher != null)
					base.NextPublisher.Publish( exception,  additionalInfo,  configSettings);
			}
			
		}
		#endregion
	}
}
