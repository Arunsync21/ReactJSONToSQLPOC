namespace JSON_to_SQL_POC
{
    partial class Form1
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
            this.jsonTextBox = new System.Windows.Forms.TextBox();
            this.sQLLabel = new System.Windows.Forms.Label();
            this.sqlQuery = new System.Windows.Forms.Label();
            this.generateButton = new System.Windows.Forms.Button();
            this.webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.btnSendMessage = new System.Windows.Forms.Button();
            this.extensionColumnsComboBox = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.webView)).BeginInit();
            this.SuspendLayout();
            // 
            // jsonTextBox
            // 
            this.jsonTextBox.Location = new System.Drawing.Point(857, 12);
            this.jsonTextBox.Multiline = true;
            this.jsonTextBox.Name = "jsonTextBox";
            this.jsonTextBox.Size = new System.Drawing.Size(354, 266);
            this.jsonTextBox.TabIndex = 0;
            // 
            // sQLLabel
            // 
            this.sQLLabel.AutoSize = true;
            this.sQLLabel.Location = new System.Drawing.Point(854, 290);
            this.sQLLabel.Name = "sQLLabel";
            this.sQLLabel.Size = new System.Drawing.Size(33, 16);
            this.sQLLabel.TabIndex = 1;
            this.sQLLabel.Text = "SQL";
            // 
            // sqlQuery
            // 
            this.sqlQuery.AutoSize = true;
            this.sqlQuery.Location = new System.Drawing.Point(893, 290);
            this.sqlQuery.Name = "sqlQuery";
            this.sqlQuery.Size = new System.Drawing.Size(0, 16);
            this.sqlQuery.TabIndex = 2;
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(1217, 146);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(114, 23);
            this.generateButton.TabIndex = 3;
            this.generateButton.Text = "Generate SQL";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // webView
            // 
            this.webView.AllowExternalDrop = true;
            this.webView.CreationProperties = null;
            this.webView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView.Location = new System.Drawing.Point(31, 21);
            this.webView.Name = "webView";
            this.webView.Size = new System.Drawing.Size(817, 693);
            this.webView.Source = new System.Uri("http://localhost:3000/", System.UriKind.Absolute);
            this.webView.TabIndex = 4;
            this.webView.ZoomFactor = 1D;
            // 
            // btnSendMessage
            // 
            this.btnSendMessage.Location = new System.Drawing.Point(1256, 189);
            this.btnSendMessage.Name = "btnSendMessage";
            this.btnSendMessage.Size = new System.Drawing.Size(75, 23);
            this.btnSendMessage.TabIndex = 5;
            this.btnSendMessage.Text = "Send Message";
            this.btnSendMessage.UseVisualStyleBackColor = true;
            this.btnSendMessage.Click += new System.EventHandler(this.btnSendMessage_Click);
            // 
            // extensionColumnsComboBox
            // 
            this.extensionColumnsComboBox.FormattingEnabled = true;
            this.extensionColumnsComboBox.Location = new System.Drawing.Point(1218, 21);
            this.extensionColumnsComboBox.Name = "extensionColumnsComboBox";
            this.extensionColumnsComboBox.Size = new System.Drawing.Size(121, 24);
            this.extensionColumnsComboBox.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1357, 819);
            this.Controls.Add(this.extensionColumnsComboBox);
            this.Controls.Add(this.btnSendMessage);
            this.Controls.Add(this.webView);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.sqlQuery);
            this.Controls.Add(this.sQLLabel);
            this.Controls.Add(this.jsonTextBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.webView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox jsonTextBox;
        private System.Windows.Forms.Label sQLLabel;
        private System.Windows.Forms.Label sqlQuery;
        private System.Windows.Forms.Button generateButton;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private System.Windows.Forms.Button btnSendMessage;
        private System.Windows.Forms.ComboBox extensionColumnsComboBox;
    }
}

