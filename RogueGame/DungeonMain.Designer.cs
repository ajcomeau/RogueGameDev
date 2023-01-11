namespace RogueGame
{
    partial class DungeonMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblStatusMsg = new System.Windows.Forms.Label();
            this.lblStats = new System.Windows.Forms.Label();
            this.lblArray = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblStatusMsg
            // 
            this.lblStatusMsg.AutoSize = true;
            this.lblStatusMsg.Font = new System.Drawing.Font("Consolas", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblStatusMsg.Location = new System.Drawing.Point(13, 9);
            this.lblStatusMsg.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStatusMsg.Name = "lblStatusMsg";
            this.lblStatusMsg.Size = new System.Drawing.Size(0, 22);
            this.lblStatusMsg.TabIndex = 0;
            // 
            // lblStats
            // 
            this.lblStats.AutoSize = true;
            this.lblStats.Font = new System.Drawing.Font("Consolas", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblStats.Location = new System.Drawing.Point(26, 589);
            this.lblStats.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(0, 22);
            this.lblStats.TabIndex = 1;
            // 
            // lblArray
            // 
            this.lblArray.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblArray.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblArray.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblArray.Location = new System.Drawing.Point(0, 0);
            this.lblArray.Name = "lblArray";
            this.lblArray.Size = new System.Drawing.Size(998, 643);
            this.lblArray.TabIndex = 2;
            this.lblArray.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnGenerate
            // 
            this.btnGenerate.ForeColor = System.Drawing.Color.Black;
            this.btnGenerate.Location = new System.Drawing.Point(885, 608);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(101, 23);
            this.btnGenerate.TabIndex = 3;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // DungeonMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(998, 643);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.lblStatusMsg);
            this.Controls.Add(this.lblArray);
            this.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DungeonMain";
            this.Text = "Rogue";
            this.Load += new System.EventHandler(this.DungeonMain_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblStatusMsg;
        private Label lblStats;
        private Label lblArray;
        private Button btnGenerate;
    }
}