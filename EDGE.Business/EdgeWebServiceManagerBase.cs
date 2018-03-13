using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using EDGE.Common;
using EDGE.Business.EdgeRegService;

namespace EDGE.Business
{
    public class EdgeWebServiceManagerBase : IDisposable
	{
		#region | Declarations |

		public const string _KEY_AUTHERIALIZE = "EdgeAutherializeKey";
		public const string _KEY_REQUEST_HEADER = "EdgeCampaignInfo";

		protected string mEdgeWebServiceUrl = string.Empty;
		protected string mUserID = string.Empty;
		protected string mPassword = string.Empty;
		bool mUseEDGEAlumniLookup = false;

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

		#region | Dispose |
		public void Dispose()
		{
		}
		#endregion

		#region | static: HasEdgeCampaignInfoContext |
		/// <summary>
		/// Method to check whether we have the header for the EdgeCampaignInfo object.
		/// </summary>
		/// <returns></returns>
		public static bool HasEdgeCampaignInfoContext()
		{
			return !string.IsNullOrWhiteSpace(HttpContext.Current?.Request?.Headers?[_KEY_REQUEST_HEADER]);
		}
		#endregion

		#region | static: EdgeCampaignInfoContext |
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
					throw;
				}
				return client;
			}
		}
		#endregion

		#region | property: RegistrationServiceClient |
		protected EdgeRegService.RegistrationServiceClient RegistrationServiceClient
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
					throw;
				}
				return client;
			}
		}
		#endregion

		#region | CampaignTokenDecode |
		public virtual EdgeCampaignInfo CampaignTokenDecode(string token, string activityCode = null, string promoCode = null)
		{
			string auth = ProcessAuth(mUserID, mPassword);

			EdgeCampaignInfo edgeCampaignInfo = new EdgeCampaignInfo()
			{
				Token = token,
				ActivityCode = activityCode,
				PromoCode = promoCode
			};

			// Set the "auth" on the object as well, so we can more quickly access in future requests.
			// TODO: Is this the best approach to take, or should we just re-auth on every call?
			edgeCampaignInfo.AuthString = auth;

			// call webservice to parse out token
			// if it returns true, then replace all properties from token
			// otherwise keep promocode and activitycode
			// if Token parse fails, log error and return false
			EdgeRegService.RegistrationServiceClient client = this.RegistrationServiceClient;

			EdgeRegService.ServiceResponseOfSecureCampaignLinkNcCATIYq td = client.CampaignTokenDecode(auth, token);

			bool bHasErrors = HandleWSErrors(td);
			if ( bHasErrors )
			{
				return edgeCampaignInfo;
			}

			// check expiration date/time
			TimeSpan span = td.ReturnValue.LinkExpires.Subtract(DateTime.Now);
			if ( span.Ticks < 0 )
			{
				EDGE.ExceptionManagement.ExceptionManager.Publish(new ApplicationException("Email Link has expired"));
				return edgeCampaignInfo;
			}

			// Success from the Web Service!
			edgeCampaignInfo.DecodeSuccess = true;

			// set decoded token variables to object properties
			edgeCampaignInfo.ActivityCode = td.ReturnValue.ActivityCode;
			edgeCampaignInfo.PromoCode = td.ReturnValue.PromoCode;
			edgeCampaignInfo.AudienceCode = td.ReturnValue.AudienceCode;
			edgeCampaignInfo.CampaignActivityID = td.ReturnValue.CampaignActivityId;
			edgeCampaignInfo.CampaignLogID = td.ReturnValue.CampaignLogId;

			// Autherialize was having some issues serializing with the MaxValue. Only set if the LinkExpires
			// property is a valid date.
			if ( td.ReturnValue.LinkExpires < DateTime.MaxValue && td.ReturnValue.LinkExpires > DateTime.MinValue )
				edgeCampaignInfo.LinkExpires = td.ReturnValue.LinkExpires;

			// make sure token is not from email campaign and registrant is registered through Edge
			if ( edgeCampaignInfo.CampaignLogID <= 0 && edgeCampaignInfo.CampaignActivityID <= 0 )
			{
				edgeCampaignInfo.WebRegSourceCode = "EDGE";
				edgeCampaignInfo.EdgeUserName = td.ReturnValue.UserName;
			}

			return edgeCampaignInfo;
		}
		#endregion

		#region | ContactSelectionByCampaignToken |
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

		#region | FillRegistrantFromEdge |
		public virtual EdgeCampaignInfo FillRegistrantFromEdge(string autherializedToken)
		{
			return FillRegistrantFromEdge(autherializedToken.DataContractDeAutherialize<EdgeCampaignInfo>(_KEY_AUTHERIALIZE));
		}

		public virtual EdgeCampaignInfo FillRegistrantFromEdge(EdgeCampaignInfo edgeCampaignInfo)
		{
			if ( edgeCampaignInfo == null )
				return null;


			// No auto-fill; just return the campaign per previous behavior.
			if ( !edgeCampaignInfo.DecodeSuccess )
				return edgeCampaignInfo;

			EdgeRegService.ContactLookupResult result = ContactSelectByCampaignToken(edgeCampaignInfo.Token);

			if ( result != null )
			{
				EdgeRegService.Contact contact = result.Contacts.FirstOrDefault();

				if ( contact != null )
				{
					EdgeRegService.CompanyContact companyContact = result.CompanyContacts.Where(a => a.ContactId == contact.ContactId).FirstOrDefault();
					EdgeRegService.ContactAddress address = result.ContactAddresses.Where(a => a.ContactId == contact.ContactId).FirstOrDefault();

					EdgeRegService.Company company = companyContact == null ? null : result.Companies.Where(a => a.CompanyId == companyContact.CompanyId).FirstOrDefault();
					EdgeRegService.Country country = address == null ? null : result.Countries.Where(a => a.CountryId == address.CountryId).FirstOrDefault();
					EdgeRegService.State state = address == null ? null : result.States.Where(a => a.StateId == address.StateId).FirstOrDefault();
				}
			}

			return edgeCampaignInfo;
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
