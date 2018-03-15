using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EDGE.Common;
using EDGE.Business.EdgeRegService;

namespace EDGE.Business
{
	public class EdgeWebService : EdgeWebServiceManagerBase
	{
		public EdgeWebService() : base()
		{
		}

		#region | Test_CompanyAdd |
		/// <summary>
		/// EventXL.Registration.Show.Business.EdgeWebServiceManager.cs
		/// I think this is where you would put your EXL -> EDGE interface code.
		/// </summary>
		/// <param name="Registrant"></param>
		public ExecutionMessage[] Test_CompanyAdd()
		{
			var Msgs = new ExecutionMessageCollection();
			try
			{
				// Transfer registrant to EDGE dto
				var company = new dtoEDGECompany()
				{
					CompanyName = "Internet Research Link",
					CompanyName2 = "",
					WebSite = "www.pdjmwj.com",
					Address1 = "6220 Main ST",
					Address2 = "",
					Address3 = "",
					City = "Mount Airy",
					StateCode = "MD",
					CountryName = "",
					PostalCode = "21771",
					Fax = "",
					Phone = "",
					PhoneExt = "",
					PhoneTollFree = "",
					MemberId = "",
				};

				int siloId = this.SelectSiloIdFromEdge("ADT121", "EventXL");

				string auth = this.ProcessAuth(mUserID, mPassword);
				if ( !string.IsNullOrEmpty(auth) )
				{
					EdgeRegService.RegistrationServiceClient client = this.RegistrationServiceClient;
					var result = client.CompanyAdd(auth, siloId, company);
					bool bHasErrors = this.HandleWSErrors(result);
					if ( !bHasErrors )
					{
						// Not sure where this is stored in EXL
						int EDGECompanyId = result.ReturnValue;
					}
				}
			}
			catch ( Exception ex )
			{
				Msgs.Add(new ErrorMessage(ex.FormatMessage("Test_CompanyAdd")));
			}
			return Msgs.ToArray();
		}
		#endregion

		#region | CompanyAdd |
		public int CompanyAdd(string showCode, dtoEDGECompany company)
		{
			int EDGECompanyId = 0;

			int siloId = this.SelectSiloIdFromEdge(showCode, "EventXL");

			string auth = this.ProcessAuth(mUserID, mPassword);
			if ( !string.IsNullOrEmpty(auth) )
			{
				EdgeRegService.RegistrationServiceClient client = this.RegistrationServiceClient;
				var result = client.CompanyAdd(auth, siloId, company);
				bool bHasErrors = this.HandleWSErrors(result);
				if ( !bHasErrors )
				{
					EDGECompanyId = result.ReturnValue;
				}
			}

			return EDGECompanyId;
		}
		#endregion

		#region | SelectSiloIdFromEdge |
		public int SelectSiloIdFromEdge(string vendorEventCode, string vendorTypeCode = "")
		{
			int siloId = 0;

			string auth = this.ProcessAuth(mUserID, mPassword);
			if ( !string.IsNullOrEmpty(auth) )
			{
				var searchCriteria = new EdgeRegService.evwEventSearchSearchCriteria();
				searchCriteria.Event_RegVendorEventCode = vendorEventCode;
				searchCriteria.Event_RegVendorEventCode_SearchType = EdgeRegService.SearchCriterionTextualType.Equal;
				searchCriteria.Event_RegVendorTypeCode = string.IsNullOrEmpty(vendorTypeCode) ? "EventXL" : vendorTypeCode;
				searchCriteria.Event_RegVendorTypeCode_SearchType = EdgeRegService.SearchCriterionTextualType.Equal;

				var searchSortOrders = new List<EdgeRegService.evwEventSearchSortOrder>();

				EdgeRegService.RegistrationServiceClient client = this.RegistrationServiceClient;
				var result = client.EventSearch(auth, searchCriteria, searchSortOrders.ToArray(), 0, 1);

				if ( result.ReturnValue != null && result.ReturnValue.Item1 > 0 )
				{
					siloId = result.ReturnValue.Item2.ToList()
						.Where(a => a.sysRowState.Equals("ACT", StringComparison.CurrentCultureIgnoreCase)).Select(a => a.SiloId)
						.FirstOrDefault();
				}
			}

			return siloId;
		}
		#endregion
	}
}
