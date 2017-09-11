using System;
using System.Collections;
using System.Windows.Forms;
using EDGE.Common;

namespace EDGE.Service.Test
{
	/// <summary>
	/// CRM Utility / Generic functions for Windows applications.
	/// </summary>
	public class WinUtility
	{
		#region | DisplayMessage |
		/// <summary>
		/// Standardize error message display.
		/// </summary>
		public static void DisplayMessage(Exception ex, string routineName)
		{
			Form Frm = null;
			WinUtility.DisplayMessage(ex, routineName, Frm);
		}
		public static void DisplayMessage(Exception ex, string routineName, Control control)
		{
			if ( String.IsNullOrEmpty(routineName) )
				routineName = control.GetType().ToString();
			else
				routineName = string.Format("{0}.{1}", control.GetType().ToString(), routineName);
			WinUtility.DisplayMessage(ex, routineName, control.FindForm());
		}
		public static void DisplayMessage(Exception ex, string routineName, Form form)
		{
			FormException Frm = null;
			try
			{
				if ( form != null )
				{
					if ( String.IsNullOrEmpty(routineName) )
						routineName = form.GetType().ToString();
					else
						routineName = string.Format("{0}.{1}", form.GetType().ToString(), routineName);
				}

				Frm = new FormException(ex, routineName);
				if ( form == null )
					Frm.ShowDialog();
				else
					Frm.ShowDialog(form);
			}
			finally
			{
				if ( Frm != null )
					Frm.Dispose();
			}
		}
		#endregion

		#region | DisplayMessage |
		/// <summary>
		/// Method to show execution messages in a dialog window
		/// </summary>
		public static void DisplayMessage(ExecutionMessageCollection msgs, string routineName)
		{
			if ( msgs.Count == 0 )
				return;
			Form Frm = null;
			WinUtility.DisplayMessage(msgs, routineName, Frm);
		}
		public static void DisplayMessage(ExecutionMessageCollection msgs, string routineName, Control control)
		{
			if ( msgs.Count == 0 )
				return;
			WinUtility.DisplayMessage(msgs, routineName, control.FindForm());
		}
		public static void DisplayMessage(ExecutionMessageCollection msgs, string routineName, Form form)
		{
			if ( msgs.Count == 0 )
				return;

			if ( routineName.IsEmpty() && form != null )
				routineName = form.Text;

			string Msgs = msgs.FormatMessage();

			if ( msgs.HasError() )
				MessageBox.Show(form, Msgs, routineName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			else if ( msgs.HasWarning() )
				MessageBox.Show(form, Msgs, routineName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			else
				MessageBox.Show(form, Msgs, routineName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		#endregion

		#region | DisplayMessage |
		/// <summary>
		/// Method to show execution messages in a dialog window
		/// </summary>
		public static void DisplayMessage(string msg, string routineName, Control control)
		{
			WinUtility.DisplayMessage(msg, routineName, control.FindForm());
		}
		public static void DisplayMessage(string msg, string routineName, Form form)
		{
			if ( msg.IsEmpty() )
				return;

			if ( form != null && string.IsNullOrEmpty(routineName) )
				routineName = form.Text;

			MessageBox.Show(form, msg, routineName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
		#endregion
	}
}
