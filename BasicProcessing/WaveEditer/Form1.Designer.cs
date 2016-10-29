namespace WaveEditer
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.start_button = new System.Windows.Forms.Button();
            this.Encode_button = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.CountUp_button = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.FFT_button = new System.Windows.Forms.Button();
            this.Triangle_button = new System.Windows.Forms.Button();
            this.Sawtoot_button = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // chart1
            // 
            chartArea3.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            this.chart1.Legends.Add(legend3);
            this.chart1.Location = new System.Drawing.Point(12, 66);
            this.chart1.Name = "chart1";
            series3.ChartArea = "ChartArea1";
            series3.Legend = "Legend1";
            series3.Name = "Series1";
            this.chart1.Series.Add(series3);
            this.chart1.Size = new System.Drawing.Size(300, 300);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            this.chart1.Click += new System.EventHandler(this.chart1_Click);
            // 
            // start_button
            // 
            this.start_button.Location = new System.Drawing.Point(12, 13);
            this.start_button.Name = "start_button";
            this.start_button.Size = new System.Drawing.Size(75, 23);
            this.start_button.TabIndex = 1;
            this.start_button.Text = "Time";
            this.start_button.UseVisualStyleBackColor = true;
            this.start_button.Click += new System.EventHandler(this.start_button_Click);
            // 
            // Encode_button
            // 
            this.Encode_button.Location = new System.Drawing.Point(93, 13);
            this.Encode_button.Name = "Encode_button";
            this.Encode_button.Size = new System.Drawing.Size(75, 23);
            this.Encode_button.TabIndex = 2;
            this.Encode_button.Text = "Encode";
            this.Encode_button.UseVisualStyleBackColor = true;
            this.Encode_button.Click += new System.EventHandler(this.Encode_button_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(248, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "label1";
            // 
            // CountUp_button
            // 
            this.CountUp_button.Location = new System.Drawing.Point(289, -2);
            this.CountUp_button.Name = "CountUp_button";
            this.CountUp_button.Size = new System.Drawing.Size(75, 23);
            this.CountUp_button.TabIndex = 4;
            this.CountUp_button.Text = "1/4正弦波";
            this.CountUp_button.UseVisualStyleBackColor = true;
            this.CountUp_button.Click += new System.EventHandler(this.CountUp_button_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(167, 381);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "label2";
            // 
            // FFT_button
            // 
            this.FFT_button.Location = new System.Drawing.Point(12, 42);
            this.FFT_button.Name = "FFT_button";
            this.FFT_button.Size = new System.Drawing.Size(75, 23);
            this.FFT_button.TabIndex = 6;
            this.FFT_button.Text = "FFT";
            this.FFT_button.UseVisualStyleBackColor = true;
            this.FFT_button.Click += new System.EventHandler(this.FFT_button_Click);
            // 
            // Triangle_button
            // 
            this.Triangle_button.Location = new System.Drawing.Point(289, 18);
            this.Triangle_button.Name = "Triangle_button";
            this.Triangle_button.Size = new System.Drawing.Size(75, 23);
            this.Triangle_button.TabIndex = 7;
            this.Triangle_button.Text = "1/4三角波";
            this.Triangle_button.UseVisualStyleBackColor = true;
            this.Triangle_button.Click += new System.EventHandler(this.Triangle_button_Click);
            // 
            // Sawtoot_button
            // 
            this.Sawtoot_button.Location = new System.Drawing.Point(289, 37);
            this.Sawtoot_button.Name = "Sawtoot_button";
            this.Sawtoot_button.Size = new System.Drawing.Size(75, 23);
            this.Sawtoot_button.TabIndex = 8;
            this.Sawtoot_button.Text = "1/4鋸切波";
            this.Sawtoot_button.UseVisualStyleBackColor = true;
            this.Sawtoot_button.Click += new System.EventHandler(this.Sawtoot_button_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 402);
            this.Controls.Add(this.Sawtoot_button);
            this.Controls.Add(this.Triangle_button);
            this.Controls.Add(this.FFT_button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CountUp_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Encode_button);
            this.Controls.Add(this.start_button);
            this.Controls.Add(this.chart1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.Button start_button;
        private System.Windows.Forms.Button Encode_button;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CountUp_button;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button FFT_button;
        private System.Windows.Forms.Button Triangle_button;
        private System.Windows.Forms.Button Sawtoot_button;
    }
}

