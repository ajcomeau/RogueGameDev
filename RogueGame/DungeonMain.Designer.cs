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
            this.components = new System.ComponentModel.Container();
            this.lblStatusMsg = new System.Windows.Forms.Label();
            this.lblStats = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.lblArray = new System.Windows.Forms.Label();
            this.pnlName = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblQuestion = new System.Windows.Forms.Label();
            this.pnlName.SuspendLayout();
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
            this.lblArray.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblArray.Name = "lblArray";
            this.lblArray.Size = new System.Drawing.Size(997, 643);
            this.lblArray.TabIndex = 10;
            this.lblArray.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // pnlName
            // 
            this.pnlName.Controls.Add(this.btnStart);
            this.pnlName.Controls.Add(this.txtName);
            this.pnlName.Controls.Add(this.lblQuestion);
            this.pnlName.Location = new System.Drawing.Point(176, 567);
            this.pnlName.Name = "pnlName";
            this.pnlName.Size = new System.Drawing.Size(655, 44);
            this.pnlName.TabIndex = 11;
            // 
            // btnStart
            // 
            this.btnStart.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnStart.Location = new System.Drawing.Point(533, 9);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtName
            // 
            this.txtName.BackColor = System.Drawing.SystemColors.ControlText;
            this.txtName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.txtName.Location = new System.Drawing.Point(239, 10);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(277, 23);
            this.txtName.TabIndex = 1;
            // 
            // lblQuestion
            // 
            this.lblQuestion.AutoSize = true;
            this.lblQuestion.Location = new System.Drawing.Point(17, 13);
            this.lblQuestion.Name = "lblQuestion";
            this.lblQuestion.Size = new System.Drawing.Size(216, 17);
            this.lblQuestion.TabIndex = 0;
            this.lblQuestion.Text = "What is your rogue\'s name?";
            // 
            // DungeonMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(997, 643);
            this.Controls.Add(this.pnlName);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.lblStatusMsg);
            this.Controls.Add(this.lblArray);
            this.Font = new System.Drawing.Font("Consolas", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DungeonMain";
            this.Text = "Dungeon Map";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DungeonMain_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DungeonMain_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.DungeonMain_KeyUp);
            this.pnlName.ResumeLayout(false);
            this.pnlName.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label lblStatusMsg;
        private Label lblStats;
        private ToolTip toolTip1;
        private Label lblArray;
        private Panel pnlName;
        private Button btnStart;
        private TextBox txtName;
        private Label lblQuestion;
    }
}