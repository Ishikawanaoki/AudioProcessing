﻿namespace BasicProcessing
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.chart1_checkBox = new System.Windows.Forms.CheckBox();
            this.chart2_checkBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "output";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 46);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "実行";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 9);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(93, 23);
            this.button3.TabIndex = 3;
            this.button3.Text = "ファイルを選択";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // chart1_checkBox
            // 
            this.chart1_checkBox.AutoSize = true;
            this.chart1_checkBox.Checked = true;
            this.chart1_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chart1_checkBox.Location = new System.Drawing.Point(256, 41);
            this.chart1_checkBox.Name = "chart1_checkBox";
            this.chart1_checkBox.Size = new System.Drawing.Size(56, 16);
            this.chart1_checkBox.TabIndex = 6;
            this.chart1_checkBox.Text = "chart1";
            this.chart1_checkBox.UseVisualStyleBackColor = true;
            this.chart1_checkBox.CheckedChanged += new System.EventHandler(this.chart1_checkBox_CheckedChanged);
            // 
            // chart2_checkBox
            // 
            this.chart2_checkBox.AutoSize = true;
            this.chart2_checkBox.Checked = true;
            this.chart2_checkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chart2_checkBox.Location = new System.Drawing.Point(256, 63);
            this.chart2_checkBox.Name = "chart2_checkBox";
            this.chart2_checkBox.Size = new System.Drawing.Size(56, 16);
            this.chart2_checkBox.TabIndex = 7;
            this.chart2_checkBox.Text = "chart2";
            this.chart2_checkBox.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 137);
            this.Controls.Add(this.chart2_checkBox);
            this.Controls.Add(this.chart1_checkBox);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.CheckBox chart1_checkBox;
        private System.Windows.Forms.CheckBox chart2_checkBox;
    }
}

