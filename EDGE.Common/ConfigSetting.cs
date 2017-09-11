using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDGE.Common
{
    public class ConfigSetting
    {
		#region | AppSettings |
		public static string AppSettings(string keyName)
		{
			return ConfigurationManager.AppSettings[keyName.Trim()];
		}
		#endregion

		#region | EnvironmentMode |
		private static string _EnvironmentMode = "";
		public static string EnvironmentMode
		{
			get
			{
				if ( _EnvironmentMode.IsEmpty() )
					_EnvironmentMode = ConfigurationManager.AppSettings["UnifiedDeployment_Environment"];
				return _EnvironmentMode;
			}
		}
		#endregion

		#region | EdgeWebServiceURL |
		private static string _EdgeWebServiceURL = "";
		public static string EdgeWebServiceURL
		{
			get
			{
				if ( _EdgeWebServiceURL.IsEmpty() )
					_EdgeWebServiceURL = ConfigurationManager.AppSettings["EdgeWebServiceURL"];
				return _EdgeWebServiceURL;
			}
		}
		#endregion

		#region | UserName |
		private static string _UserName = "";
		public static string UserName
		{
			get
			{
				if ( _UserName.IsEmpty() )
				{
					string User = System.Environment.UserName;
					int pos = User.LastIndexOf(@"\");
					if ( pos > -1 )
						User = User.Substring(pos);
					_UserName = User.ToLower();
				}
				return _UserName;
			}
		}
		#endregion
	}
}
