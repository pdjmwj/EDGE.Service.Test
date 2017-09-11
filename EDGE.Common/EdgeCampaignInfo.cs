using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDGE.Common
{
	[Serializable]
	public class EdgeCampaignInfo
	{
		public EdgeCampaignInfo()
		{
			this.Token = string.Empty;
			this.ActivityCode = string.Empty;
			this.PromoCode = string.Empty;
			this.LinkExpires = DateTime.MinValue;
			this.AudienceCode = string.Empty;
			this.CampaignActivityID = -1;
			this.CampaignLogID = -1;
			this.EdgeContactID = string.Empty;
			this.EdgeCompanyID = string.Empty;
			this.AuthString = string.Empty;
			this.WebRegSourceCode = string.Empty;
			this.QCGroupNumber = string.Empty;
			this.EdgeUserName = string.Empty;
			this.DecodeSuccess = false;
		}

		public string Token { get; set; }
		public string ActivityCode { get; set; }
		public string PromoCode { get; set; }
		public DateTime LinkExpires { get; set; }
		public string AudienceCode { get; set; }
		public int? CampaignActivityID { get; set; }
		public int? CampaignLogID { get; set; }
		public string EdgeContactID { get; set; }
		public string EdgeCompanyID { get; set; }
		public string AuthString { get; set; }
		public string WebRegSourceCode { get; set; }
		public string QCGroupNumber { get; set; }
		public string EdgeUserName { get; set; }

		/// <summary>
		/// There is some legacy logic that if the web service fails, we still
		/// keep the ActivityCode and PromoCode if they were passed.
		/// This property will tell us if campaign decoded successfully.
		/// </summary>
		public bool DecodeSuccess { get; set; }
	}
}
