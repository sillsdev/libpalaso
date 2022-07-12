
namespace SIL.Windows.Forms.TestApp
{
	partial class ModalChildForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.lblCheckedIndices = new System.Windows.Forms.Label();
			this.lblCheckedIndicesData = new System.Windows.Forms.Label();
			this.chkUseCheckedComboBoxItems = new System.Windows.Forms.CheckBox();
			this.cboWhiteSpaceCharacters = new SIL.Windows.Forms.CheckedComboBox.CheckedComboBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(127, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Hi. I\'m a modal child form.";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(15, 34);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(386, 20);
			this.textBox1.TabIndex = 1;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(326, 131);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(245, 131);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			// 
			// lblCheckedIndices
			// 
			this.lblCheckedIndices.AutoSize = true;
			this.lblCheckedIndices.Location = new System.Drawing.Point(211, 94);
			this.lblCheckedIndices.Name = "lblCheckedIndices";
			this.lblCheckedIndices.Size = new System.Drawing.Size(89, 13);
			this.lblCheckedIndices.TabIndex = 5;
			this.lblCheckedIndices.Text = "Checked indices:";
			// 
			// lblCheckedIndicesData
			// 
			this.lblCheckedIndicesData.AutoSize = true;
			this.lblCheckedIndicesData.Location = new System.Drawing.Point(306, 94);
			this.lblCheckedIndicesData.Name = "lblCheckedIndicesData";
			this.lblCheckedIndicesData.Size = new System.Drawing.Size(0, 13);
			this.lblCheckedIndicesData.TabIndex = 6;
			// 
			// chkUseCheckedComboBoxItems
			// 
			this.chkUseCheckedComboBoxItems.AutoSize = true;
			this.chkUseCheckedComboBoxItems.Checked = true;
			this.chkUseCheckedComboBoxItems.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkUseCheckedComboBoxItems.Location = new System.Drawing.Point(15, 68);
			this.chkUseCheckedComboBoxItems.Name = "chkUseCheckedComboBoxItems";
			this.chkUseCheckedComboBoxItems.Size = new System.Drawing.Size(241, 17);
			this.chkUseCheckedComboBoxItems.TabIndex = 7;
			this.chkUseCheckedComboBoxItems.Text = "Checkbox items are CheckedComboBoxItems";
			this.chkUseCheckedComboBoxItems.UseVisualStyleBackColor = true;
			this.chkUseCheckedComboBoxItems.CheckedChanged += new System.EventHandler(this.chkUseCheckedComboBoxItems_CheckedChanged);
			// 
			// cboWhiteSpaceCharacters
			// 
			this.cboWhiteSpaceCharacters.CheckOnClick = true;
			this.cboWhiteSpaceCharacters.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.cboWhiteSpaceCharacters.DropDownHeight = 1;
			this.cboWhiteSpaceCharacters.DropDownWidth = 220;
			this.cboWhiteSpaceCharacters.FormattingEnabled = true;
			this.cboWhiteSpaceCharacters.IntegralHeight = false;
			this.cboWhiteSpaceCharacters.Location = new System.Drawing.Point(15, 91);
			this.cboWhiteSpaceCharacters.Name = "cboWhiteSpaceCharacters";
			this.cboWhiteSpaceCharacters.Size = new System.Drawing.Size(190, 21);
			this.cboWhiteSpaceCharacters.SummaryDisplayMember = "ArgbValue";
			this.cboWhiteSpaceCharacters.TabIndex = 4;
			this.cboWhiteSpaceCharacters.ValueSeparator = ", ";
			this.cboWhiteSpaceCharacters.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.CboWhiteSpaceCharactersOnItemChecked);
			// 
			// ModalChildForm
			// 
			this.AcceptButton = this.btnCancel;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnOk;
			this.ClientSize = new System.Drawing.Size(413, 166);
			this.Controls.Add(this.chkUseCheckedComboBoxItems);
			this.Controls.Add(this.lblCheckedIndicesData);
			this.Controls.Add(this.lblCheckedIndices);
			this.Controls.Add(this.cboWhiteSpaceCharacters);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label1);
			this.Name = "ModalChildForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "ModalChildForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private CheckedComboBox.CheckedComboBox cboWhiteSpaceCharacters;
		private System.Windows.Forms.Label lblCheckedIndices;
		private System.Windows.Forms.Label lblCheckedIndicesData;
		private System.Windows.Forms.CheckBox chkUseCheckedComboBoxItems;
	}
}