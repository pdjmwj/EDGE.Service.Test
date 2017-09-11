using System;
using System.Windows.Forms;

namespace EDGE.Service.Test
{
	public partial class FormMessage : Form
	{
		public FormMessage()
		{
			InitializeComponent();
			this.MessageAnswer.Visible = false;
			this.ButtonOK.Visible = false;
		}
		public FormMessage(bool IsAsking)
		{
			InitializeComponent();
			this.MessageAnswer.Visible = IsAsking;
			this.ButtonOK.Visible = IsAsking;
			if ( IsAsking )
				this.ButtonClose.Text = "Cancel";
		}

		private void ButtonClose_Click(object sender, EventArgs e)
		{
			InnerIsCancelled = true;
			this.Close();
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			InnerIsCancelled = false;
			this.Close();
		}

		private bool InnerIsCancelled = true;
		public bool MessageCancelled
		{
			get { return InnerIsCancelled; }
		}
	}
}
