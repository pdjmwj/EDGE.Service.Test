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

		#region | CompanyAdd |
		public int CompanyAdd(string showCode, dtoEDGECompany company)
		{
			int CompanyId = 0;

			int siloId = this.SelectSiloIdFromEdge(showCode, "EventXL");

			string auth = this.ProcessAuth(mUserID, mPassword);
			if ( !string.IsNullOrEmpty(auth) )
			{
				EdgeRegService.RegistrationServiceClient client = this.RegistrationServiceClient;
				var result = client.CompanyAdd(auth, siloId, company);
				bool bHasErrors = HandleWSErrors(result);
				if ( !bHasErrors )
				{
					CompanyId = result.ReturnValue;
				}
			}

			return CompanyId;
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
