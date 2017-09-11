using EDGE.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace EDGE.Business
{
    public class EdgeWebServiceManagerBase
	{
		#region | Declarations |

		public const string _KEY_AUTHERIALIZE = "EdgeAutherializeKey";
		public const string _KEY_REQUEST_HEADER = "EdgeCampaignInfo";

		protected string mEdgeWebServiceUrl = string.Empty;
		protected string mUserID = string.Empty;
		protected string mPassword = string.Empty;
		bool mUseEDGEAlumniLookup = false;

		//private RegistrantManagerBase InnerRegistrantManager;

		#endregion

		#region | Construction |
		public EdgeWebServiceManagerBase()
		{
			this.mEdgeWebServiceUrl = ConfigSetting.AppSettings("EdgeWebServiceURL");
			this.mUserID = ConfigSetting.AppSettings("EdgeUserID");
			this.mPassword = ConfigSetting.AppSettings("EdgePassword");
			this.mUseEDGEAlumniLookup = ConfigSetting.AppSettings("UseEDGEAlumniLookup") == "1" ? true : false;

			//this.InnerRegistrantManager = new RegistrantManagerBase();
		}
		#endregion

		#region | HasEdgeCampaignInfoContext |
		/// <summary>
		/// Method to check whether we have the header for the EdgeCampaignInfo object.
		/// </summary>
		/// <returns></returns>
		public static bool HasEdgeCampaignInfoContext()
		{
			return !string.IsNullOrWhiteSpace(HttpContext.Current?.Request?.Headers?[_KEY_REQUEST_HEADER]);
		}
		#endregion

		#region | EdgeCampaignInfoContext |
		public static EdgeCampaignInfo EdgeCampaignInfoContext
		{
			get
			{
				if ( HasEdgeCampaignInfoContext() )
				{
					var header = HttpContext.Current.Request.Headers[_KEY_REQUEST_HEADER];
					var edgeCampaignInfo = header.DataContractDeAutherialize<EdgeCampaignInfo>(_KEY_AUTHERIALIZE);
					if ( edgeCampaignInfo != null )
						return edgeCampaignInfo;
				}

				// Should probably return null, but to be consistent with previous behavior...
				return new EdgeCampaignInfo();
			}
		}
		#endregion

		#region | property: AuthenticationServiceClient |
		private EdgeAuthService.AuthenticationServiceClient AuthenticationServiceClient
		{
			get
			{
				EdgeAuthService.AuthenticationServiceClient client = null;
				try
				{
					if ( !string.IsNullOrEmpty(mEdgeWebServiceUrl) )
					{
						string uri = mEdgeWebServiceUrl.TrimEnd('/') + "/AuthenticationService.svc/soap";

						client = new EdgeAuthService.AuthenticationServiceClient();

						System.ServiceModel.BasicHttpBinding binding = client.Endpoint.Binding as System.ServiceModel.BasicHttpBinding;
						SetupWSSecurityMode(uri, ref binding);

						client.Endpoint.Address = new System.ServiceModel.EndpointAddress(uri);
					}
				}
				catch
				{
				}
				return client;
			}
		}
		#endregion

		#region | property: RegistrationServiceClient |
		private EdgeRegService.RegistrationServiceClient RegistrationServiceClient
		{
			get
			{
				EdgeRegService.RegistrationServiceClient client = null;

				try
				{
					if ( !string.IsNullOrEmpty(mEdgeWebServiceUrl) )
					{
						// get webservice addres
						string uri = mEdgeWebServiceUrl.TrimEnd('/') + "/RegistrationService.svc/soap";

						client = new EdgeRegService.RegistrationServiceClient();

						System.ServiceModel.BasicHttpBinding binding = client.Endpoint.Binding as System.ServiceModel.BasicHttpBinding;
						SetupWSSecurityMode(uri, ref binding);

						client.Endpoint.Address = new System.ServiceModel.EndpointAddress(uri);
					}
				}
				catch
				{
				}

				return client;
			}
		}
		#endregion

		#region | ProcessAuth |
		protected virtual string ProcessAuth(string userId, string password)
		{
			// If we have set the Auth String already as part of the autherialized token, then 
			// use it if we have it.
			// TODO: Re-evaluate this behavior...
			var savedAuthString = EdgeWebServiceManagerBase.EdgeCampaignInfoContext.AuthString;
			if ( !string.IsNullOrWhiteSpace(savedAuthString) )
				return savedAuthString;

			EdgeAuthService.AuthenticationServiceClient client = this.AuthenticationServiceClient;

			string authString = "";
			if ( client != null )
			{
				EdgeAuthService.ServiceResponseOfstring auth = client.LoginByUsernamePassword(userId, password, 1800);

				bool bHasErrors = HandleWSErrors(auth);
				if ( bHasErrors )
					return "";

				authString = auth.ReturnValue;
			}
			return authString;
		}
		#endregion

		#region | ContactSelectionByCampaignToken |
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Token"></param>
		/// <returns></returns>
		public virtual EdgeRegService.ContactLookupResult ContactSelectByCampaignToken(string Token)
		{
			EdgeRegService.RegistrationServiceClient client = this.RegistrationServiceClient;

			string auth = ProcessAuth(mUserID, mPassword);

			if ( !string.IsNullOrEmpty(auth) )
			{
				EdgeRegService.ServiceResponseOfContactLookupResultNcCATIYq result = client.CampaignTokenContactInfo(auth, Token);

				bool bHasErrors = HandleWSErrors(result);

				if ( !bHasErrors && result.ReturnValue != null )
				{
					return result.ReturnValue;
				}
			}
			return null;
		}
		#endregion

		#region | SetupWSSecurityMode |
		private void SetupWSSecurityMode(string ServiceURL, ref System.ServiceModel.BasicHttpBinding binding)
		{
			if ( binding != null )
			{
				if ( ServiceURL.IndexOf("https", StringComparison.CurrentCultureIgnoreCase) >= 0 )
					binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
				else
					binding.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.None;
			}
		}
		#endregion

		#region | HandleWSErrors |
		protected virtual bool HandleWSErrors(EdgeAuthService.ServiceResponse result)
		{
			bool bHasError = false;
			if ( result.ExecMsgs.Any() )
			{
				StringBuilder sb = new StringBuilder();
				var Errs = result.ExecMsgs.Select(a => a.Message).ToArray();
				foreach ( string s in Errs )
				{
					sb.Append(s + " ");
				}
				EDGE.ExceptionManagement.ExceptionManager.Publish(new ApplicationException(sb.ToString()));
				bHasError = true;
			}
			return bHasError;
		}

		protected virtual bool HandleWSErrors(EdgeRegService.ServiceResponse result)
		{
			bool bHasError = false;
			if ( result.ExecMsgs.Any() )
			{
				StringBuilder sb = new StringBuilder();
				var Errs = result.ExecMsgs.Select(a => a.Message).ToArray();
				foreach ( string s in Errs )
				{
					sb.Append(s + " ");
				}
				EDGE.ExceptionManagement.ExceptionManager.Publish(new ApplicationException(sb.ToString()));
				bHasError = true;
			}
			return bHasError;
		}
		#endregion
	}
}
