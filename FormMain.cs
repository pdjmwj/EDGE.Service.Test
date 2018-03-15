using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EDGE.Business;
using EDGE.Business.EdgeRegService;
using EDGE.Common;

namespace EDGE.Service.Test
{
	public partial class FormMain : Form
	{
		#region | Construction |
		public FormMain()
		{
			InitializeComponent();
		}
		#endregion

		#region | buttonTest_Click |
		private void buttonTest_Click(object sender, EventArgs e)
		{
			this.Test_CompanyAdd();
		}
		#endregion

		#region | Test_CompanyAdd |
		private void Test_CompanyAdd()
		{
			var Msgs = new ExecutionMessageCollection();
			try
			{
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

				using ( EdgeWebService svc = new EdgeWebService() )
				{
					int EDGECompanyId = svc.CompanyAdd("ADT121", company);
				}
			}
			catch ( Exception ex )
			{
				Msgs.Add(new ErrorMessage(ex.FormatMessage("Test_CompanyAdd")));
			}
			this.DisplayMessage(Msgs);
		}
		#endregion

		#region | Test_FillRegistrantFromEdge |
		private void Test_FillRegistrantFromEdge()
		{
			//https://qawebreg.experientevent.com/showafh622?ref=828T&token=BALL6iTG3NB3xmacCkBizPlBPihZFX13VH4Hzq-o6WT.wQnbIQ__

			const string token = @"BALL6iTG3NB3xmacCkBizPlBPihZFX13VH4Hzq-o6WT.wQnbIQ__";

			var Msgs = new ExecutionMessageCollection();
			try
			{
				using ( EdgeWebService svc = new EdgeWebService() )
				{
					EdgeCampaignInfo edgeCampaignInfo = svc.CampaignTokenDecode(token, "", "");
					EdgeCampaignInfo eci = svc.FillRegistrantFromEdge(edgeCampaignInfo);
				}
			}
			catch ( Exception ex )
			{
				Msgs.Add(new ErrorMessage(ex.FormatMessage("Test_FillRegistrantFromEdge")));
			}
			this.DisplayMessage(Msgs);
		}
		#endregion

		#region | DisplayMessage |
		/// <summary>
		/// Displays the text message to the user
		/// </summary>
		/// <param name="text">The text to be displayed to the user.</param>
		protected void DisplayMessage(string text)
		{
			if ( string.IsNullOrWhiteSpace(text) )
				return;
			var msgs = new ExecutionMessageCollection() { new UserMessage(text) };
			WinUtility.DisplayMessage(msgs, this.Text, this);
		}
		/// <summary>
		/// Standardize error message display.
		/// </summary>
		protected void DisplayMessage(Exception ex, string routineName)
		{
			if ( String.IsNullOrEmpty(routineName) )
				routineName = this.Text;
			WinUtility.DisplayMessage(ex, routineName, this);
		}
		/// <summary>
		/// Standardize execution message display.
		/// Formats and displays all Excution Messages in the collection.
		/// </summary>
		protected void DisplayMessage(ExecutionMessageCollection executionMessages)
		{
			WinUtility.DisplayMessage(executionMessages, this.Text, this);
		}
		/// <summary>
		/// Standardize execution message display.
		/// Formats and displays all Excution Messages in the collection.
		/// </summary>
		protected void DisplayMessage(ExecutionMessageCollection executionMessages, string routineName)
		{
			WinUtility.DisplayMessage(executionMessages, routineName, this);
		}
		#endregion
	}
}
