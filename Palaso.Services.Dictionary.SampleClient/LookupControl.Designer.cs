namespace Palaso.DictionaryService.SampleClient
{
	partial class LookupControl
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._choicesList = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this._word = new System.Windows.Forms.TextBox();
			this._entryViewer = new System.Windows.Forms.WebBrowser();
			this._findSimilarButton = new System.Windows.Forms.Button();
			this._jumpLink = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			//
			// _choicesList
			//
			this._choicesList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this._choicesList.FormattingEnabled = true;
			this._choicesList.Location = new System.Drawing.Point(53, 40);
			this._choicesList.Name = "_choicesList";
			this._choicesList.Size = new System.Drawing.Size(96, 121);
			this._choicesList.TabIndex = 2;
			this._choicesList.SelectedIndexChanged += new System.EventHandler(this.OnChoicesList_SelectedIndexChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(14, 14);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(33, 13);
			this.label2.TabIndex = 11;
			this.label2.Text = "Word";
			//
			// _word
			//
			this._word.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
			this._word.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.HistoryList;
			this._word.Location = new System.Drawing.Point(53, 11);
			this._word.Name = "_word";
			this._word.Size = new System.Drawing.Size(97, 20);
			this._word.TabIndex = 0;
			this._word.Text = "aari";
			this._word.TextChanged += new System.EventHandler(this.OnWord_TextChanged);
			//
			// _entryViewer
			//
			this._entryViewer.AllowWebBrowserDrop = false;
			this._entryViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._entryViewer.Location = new System.Drawing.Point(176, 43);
			this._entryViewer.MinimumSize = new System.Drawing.Size(20, 20);
			this._entryViewer.Name = "_entryViewer";
			this._entryViewer.Size = new System.Drawing.Size(220, 118);
			this._entryViewer.TabIndex = 4;
			//
			// _findSimilarButton
			//
			this._findSimilarButton.Location = new System.Drawing.Point(176, 11);
			this._findSimilarButton.Name = "_findSimilarButton";
			this._findSimilarButton.Size = new System.Drawing.Size(66, 23);
			this._findSimilarButton.TabIndex = 1;
			this._findSimilarButton.Text = "Find";
			this._findSimilarButton.UseVisualStyleBackColor = true;
			this._findSimilarButton.Click += new System.EventHandler(this.OnFindSimilarButton_Click);
			//
			// _jumpLink
			//
			this._jumpLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._jumpLink.AutoSize = true;
			this._jumpLink.Location = new System.Drawing.Point(50, 171);
			this._jumpLink.Name = "_jumpLink";
			this._jumpLink.Size = new System.Drawing.Size(53, 13);
			this._jumpLink.TabIndex = 3;
			this._jumpLink.TabStop = true;
			this._jumpLink.Text = "Jump to...";
			this._jumpLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnJumpLink_LinkClicked);
			//
			// LookupControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._jumpLink);
			this.Controls.Add(this._choicesList);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._word);
			this.Controls.Add(this._entryViewer);
			this.Controls.Add(this._findSimilarButton);
			this.Name = "LookupControl";
			this.Size = new System.Drawing.Size(414, 198);
			this.Load += new System.EventHandler(this.LookupControl_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListBox _choicesList;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox _word;
		private System.Windows.Forms.WebBrowser _entryViewer;
		private System.Windows.Forms.Button _findSimilarButton;
		private System.Windows.Forms.LinkLabel _jumpLink;
	}
}
