namespace EDGE.Service.Test
{
	partial class FormMessage
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if ( disposing && (components != null) )
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.Message = new System.Windows.Forms.Label();
			this.ButtonClose = new System.Windows.Forms.Button();
			this.MessageAnswer = new System.Windows.Forms.TextBox();
			this.ButtonOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// Message
			// 
			this.Message.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Message.Location = new System.Drawing.Point(13, 13);
			this.Message.Margin = new System.Windows.Forms.Padding(3);
			this.Message.Name = "Message";
			this.Message.Size = new System.Drawing.Size(356, 183);
			this.Message.TabIndex = 0;
			this.Message.Text = "Text Message";
			this.Message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// ButtonClose
			// 
			this.ButtonClose.Location = new System.Drawing.Point(294, 201);
			this.ButtonClose.Name = "ButtonClose";
			this.ButtonClose.Size = new System.Drawing.Size(75, 23);
			this.ButtonClose.TabIndex = 3;
			this.ButtonClose.Text = "Close";
			this.ButtonClose.UseVisualStyleBackColor = true;
			this.ButtonClose.Click += new System.EventHandler(this.ButtonClose_Click);
			// 
			// MessageAnswer
			// 
			this.MessageAnswer.Location = new System.Drawing.Point(13, 202);
			this.MessageAnswer.Name = "MessageAnswer";
			this.MessageAnswer.Size = new System.Drawing.Size(194, 22);
			this.MessageAnswer.TabIndex = 1;
			// 
			// ButtonOK
			// 
			this.ButtonOK.Location = new System.Drawing.Point(213, 201);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = new System.Drawing.Size(75, 23);
			this.ButtonOK.TabIndex = 2;
			this.ButtonOK.Text = "Ok";
			this.ButtonOK.UseVisualStyleBackColor = true;
			this.ButtonOK.Click += new System.EventHandler(this.ButtonOK_Click);
			// 
			// FormMessage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(382, 233);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.MessageAnswer);
			this.Controls.Add(this.ButtonClose);
			this.Controls.Add(this.Message);
			this.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormMessage";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "System Message";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public System.Windows.Forms.Label Message;
		public System.Windows.Forms.TextBox MessageAnswer;
		public System.Windows.Forms.Button ButtonClose;
		public System.Windows.Forms.Button ButtonOK;
	}
}