namespace GoogleVoiceResponder
{
    partial class TestForm
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
            this.btnTestCall = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPhoneNumber = new System.Windows.Forms.TextBox();
            this.btnHangup = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnTestCall
            // 
            this.btnTestCall.Location = new System.Drawing.Point(12, 46);
            this.btnTestCall.Name = "btnTestCall";
            this.btnTestCall.Size = new System.Drawing.Size(75, 23);
            this.btnTestCall.TabIndex = 19;
            this.btnTestCall.Text = "Test &Call";
            this.btnTestCall.UseVisualStyleBackColor = true;
            this.btnTestCall.Click += new System.EventHandler(this.btnTestCall_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 17);
            this.label1.TabIndex = 21;
            this.label1.Text = "Phone #";
            // 
            // txtPhoneNumber
            // 
            this.txtPhoneNumber.Location = new System.Drawing.Point(89, 9);
            this.txtPhoneNumber.Name = "txtPhoneNumber";
            this.txtPhoneNumber.Size = new System.Drawing.Size(157, 22);
            this.txtPhoneNumber.TabIndex = 20;
            // 
            // btnHangup
            // 
            this.btnHangup.Location = new System.Drawing.Point(128, 46);
            this.btnHangup.Name = "btnHangup";
            this.btnHangup.Size = new System.Drawing.Size(75, 23);
            this.btnHangup.TabIndex = 22;
            this.btnHangup.Text = "&Hangup";
            this.btnHangup.UseVisualStyleBackColor = true;
            this.btnHangup.Click += new System.EventHandler(this.btnHangup_Click);
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 103);
            this.Controls.Add(this.btnHangup);
            this.Controls.Add(this.btnTestCall);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPhoneNumber);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnTestCall;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPhoneNumber;
        private System.Windows.Forms.Button btnHangup;
    }
}