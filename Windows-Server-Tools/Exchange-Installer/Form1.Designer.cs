﻿namespace Exchange_Installer
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.DomainNameLabel = new System.Windows.Forms.Label();
            this.FirstStepLabel = new System.Windows.Forms.Label();
            this.SecondStepLabel = new System.Windows.Forms.Label();
            this.ThirdStepLabel = new System.Windows.Forms.Label();
            this.MainProgressBar = new System.Windows.Forms.ProgressBar();
            this.FirstToSecondLabel = new System.Windows.Forms.Label();
            this.SecondToThirdLabel = new System.Windows.Forms.Label();
            this.FirstToThirdLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.FourthStepTimeLabel = new System.Windows.Forms.Label();
            this.FullyReadyTimeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(16, 96);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(1613, 30);
            this.textBox1.TabIndex = 0;
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(16, 141);
            this.OKButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(567, 54);
            this.OKButton.TabIndex = 1;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // DomainNameLabel
            // 
            this.DomainNameLabel.AutoSize = true;
            this.DomainNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F);
            this.DomainNameLabel.Location = new System.Drawing.Point(16, 26);
            this.DomainNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DomainNameLabel.Name = "DomainNameLabel";
            this.DomainNameLabel.Size = new System.Drawing.Size(425, 37);
            this.DomainNameLabel.TabIndex = 2;
            this.DomainNameLabel.Text = "Please enter a domain name";
            // 
            // FirstStepLabel
            // 
            this.FirstStepLabel.AutoSize = true;
            this.FirstStepLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.FirstStepLabel.Location = new System.Drawing.Point(75, 216);
            this.FirstStepLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FirstStepLabel.Name = "FirstStepLabel";
            this.FirstStepLabel.Size = new System.Drawing.Size(45, 36);
            this.FirstStepLabel.TabIndex = 3;
            this.FirstStepLabel.Text = "---";
            // 
            // SecondStepLabel
            // 
            this.SecondStepLabel.AutoSize = true;
            this.SecondStepLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.SecondStepLabel.Location = new System.Drawing.Point(75, 292);
            this.SecondStepLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SecondStepLabel.Name = "SecondStepLabel";
            this.SecondStepLabel.Size = new System.Drawing.Size(45, 36);
            this.SecondStepLabel.TabIndex = 4;
            this.SecondStepLabel.Text = "---";
            // 
            // ThirdStepLabel
            // 
            this.ThirdStepLabel.AutoSize = true;
            this.ThirdStepLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.ThirdStepLabel.Location = new System.Drawing.Point(75, 409);
            this.ThirdStepLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ThirdStepLabel.Name = "ThirdStepLabel";
            this.ThirdStepLabel.Size = new System.Drawing.Size(45, 36);
            this.ThirdStepLabel.TabIndex = 5;
            this.ThirdStepLabel.Text = "---";
            // 
            // MainProgressBar
            // 
            this.MainProgressBar.Location = new System.Drawing.Point(617, 136);
            this.MainProgressBar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MainProgressBar.Name = "MainProgressBar";
            this.MainProgressBar.Size = new System.Drawing.Size(1013, 59);
            this.MainProgressBar.TabIndex = 6;
            // 
            // FirstToSecondLabel
            // 
            this.FirstToSecondLabel.AutoSize = true;
            this.FirstToSecondLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.FirstToSecondLabel.ForeColor = System.Drawing.Color.Red;
            this.FirstToSecondLabel.Location = new System.Drawing.Point(77, 355);
            this.FirstToSecondLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FirstToSecondLabel.Name = "FirstToSecondLabel";
            this.FirstToSecondLabel.Size = new System.Drawing.Size(33, 25);
            this.FirstToSecondLabel.TabIndex = 7;
            this.FirstToSecondLabel.Text = "---";
            // 
            // SecondToThirdLabel
            // 
            this.SecondToThirdLabel.AutoSize = true;
            this.SecondToThirdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.SecondToThirdLabel.ForeColor = System.Drawing.Color.Red;
            this.SecondToThirdLabel.Location = new System.Drawing.Point(77, 472);
            this.SecondToThirdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.SecondToThirdLabel.Name = "SecondToThirdLabel";
            this.SecondToThirdLabel.Size = new System.Drawing.Size(33, 25);
            this.SecondToThirdLabel.TabIndex = 8;
            this.SecondToThirdLabel.Text = "---";
            // 
            // FirstToThirdLabel
            // 
            this.FirstToThirdLabel.AutoSize = true;
            this.FirstToThirdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.FirstToThirdLabel.ForeColor = System.Drawing.Color.Red;
            this.FirstToThirdLabel.Location = new System.Drawing.Point(77, 514);
            this.FirstToThirdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FirstToThirdLabel.Name = "FirstToThirdLabel";
            this.FirstToThirdLabel.Size = new System.Drawing.Size(33, 25);
            this.FirstToThirdLabel.TabIndex = 9;
            this.FirstToThirdLabel.Text = "---";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(477, 709);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(608, 25);
            this.label1.TabIndex = 10;
            this.label1.Text = "We do not guarantee success installs, we are not responsible if it fails";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // FourthStepTimeLabel
            // 
            this.FourthStepTimeLabel.AutoSize = true;
            this.FourthStepTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
            this.FourthStepTimeLabel.Location = new System.Drawing.Point(75, 560);
            this.FourthStepTimeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FourthStepTimeLabel.Name = "FourthStepTimeLabel";
            this.FourthStepTimeLabel.Size = new System.Drawing.Size(45, 36);
            this.FourthStepTimeLabel.TabIndex = 11;
            this.FourthStepTimeLabel.Text = "---";
            // 
            // FullyReadyTimeLabel
            // 
            this.FullyReadyTimeLabel.AutoSize = true;
            this.FullyReadyTimeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.FullyReadyTimeLabel.ForeColor = System.Drawing.Color.Red;
            this.FullyReadyTimeLabel.Location = new System.Drawing.Point(77, 625);
            this.FullyReadyTimeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FullyReadyTimeLabel.Name = "FullyReadyTimeLabel";
            this.FullyReadyTimeLabel.Size = new System.Drawing.Size(33, 25);
            this.FullyReadyTimeLabel.TabIndex = 12;
            this.FullyReadyTimeLabel.Text = "---";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.ClientSize = new System.Drawing.Size(1647, 764);
            this.Controls.Add(this.FullyReadyTimeLabel);
            this.Controls.Add(this.FourthStepTimeLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.FirstToThirdLabel);
            this.Controls.Add(this.SecondToThirdLabel);
            this.Controls.Add(this.FirstToSecondLabel);
            this.Controls.Add(this.MainProgressBar);
            this.Controls.Add(this.ThirdStepLabel);
            this.Controls.Add(this.SecondStepLabel);
            this.Controls.Add(this.FirstStepLabel);
            this.Controls.Add(this.DomainNameLabel);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.textBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "Windows Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Label DomainNameLabel;
        private System.Windows.Forms.Label FirstStepLabel;
        private System.Windows.Forms.Label SecondStepLabel;
        private System.Windows.Forms.Label ThirdStepLabel;
        private System.Windows.Forms.ProgressBar MainProgressBar;
        private System.Windows.Forms.Label FirstToSecondLabel;
        private System.Windows.Forms.Label SecondToThirdLabel;
        private System.Windows.Forms.Label FirstToThirdLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label FourthStepTimeLabel;
        private System.Windows.Forms.Label FullyReadyTimeLabel;
    }
}

