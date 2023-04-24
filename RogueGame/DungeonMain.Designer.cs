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
            components = new System.ComponentModel.Container();
            lblStatusMsg = new Label();
            lblStats = new Label();
            toolTip1 = new ToolTip(components);
            lblArray = new Label();
            pnlName = new Panel();
            btnStart = new Button();
            txtName = new TextBox();
            lblQuestion = new Label();
            pnlName.SuspendLayout();
            SuspendLayout();
            // 
            // lblStatusMsg
            // 
            lblStatusMsg.AutoSize = true;
            lblStatusMsg.Font = new Font("Consolas", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblStatusMsg.Location = new Point(13, 9);
            lblStatusMsg.Margin = new Padding(4, 0, 4, 0);
            lblStatusMsg.Name = "lblStatusMsg";
            lblStatusMsg.Size = new Size(0, 22);
            lblStatusMsg.TabIndex = 0;
            // 
            // lblStats
            // 
            lblStats.AutoSize = true;
            lblStats.Font = new Font("Consolas", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblStats.Location = new Point(13, 624);
            lblStats.Margin = new Padding(4, 0, 4, 0);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(80, 22);
            lblStats.TabIndex = 1;
            lblStats.Text = "       ";
            // 
            // lblArray
            // 
            lblArray.BorderStyle = BorderStyle.FixedSingle;
            lblArray.Dock = DockStyle.Fill;
            lblArray.Font = new Font("Consolas", 15.75F, FontStyle.Regular, GraphicsUnit.Point);
            lblArray.Location = new Point(0, 0);
            lblArray.Margin = new Padding(4, 0, 4, 0);
            lblArray.Name = "lblArray";
            lblArray.Size = new Size(997, 655);
            lblArray.TabIndex = 10;
            // 
            // pnlName
            // 
            pnlName.Controls.Add(btnStart);
            pnlName.Controls.Add(txtName);
            pnlName.Controls.Add(lblQuestion);
            pnlName.Location = new Point(176, 567);
            pnlName.Name = "pnlName";
            pnlName.Size = new Size(655, 44);
            pnlName.TabIndex = 11;
            // 
            // btnStart
            // 
            btnStart.BackColor = SystemColors.ActiveCaptionText;
            btnStart.Location = new Point(533, 9);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(75, 23);
            btnStart.TabIndex = 2;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = false;
            btnStart.Click += btnStart_Click;
            // 
            // txtName
            // 
            txtName.BackColor = SystemColors.ControlText;
            txtName.ForeColor = Color.FromArgb(255, 128, 0);
            txtName.Location = new Point(239, 10);
            txtName.Name = "txtName";
            txtName.Size = new Size(277, 23);
            txtName.TabIndex = 1;
            // 
            // lblQuestion
            // 
            lblQuestion.AutoSize = true;
            lblQuestion.Location = new Point(17, 13);
            lblQuestion.Name = "lblQuestion";
            lblQuestion.Size = new Size(216, 17);
            lblQuestion.TabIndex = 0;
            lblQuestion.Text = "What is your rogue's name?";
            // 
            // DungeonMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(997, 655);
            Controls.Add(pnlName);
            Controls.Add(lblStats);
            Controls.Add(lblStatusMsg);
            Controls.Add(lblArray);
            Font = new Font("Consolas", 10F, FontStyle.Bold, GraphicsUnit.Point);
            ForeColor = Color.FromArgb(255, 128, 0);
            KeyPreview = true;
            Margin = new Padding(4);
            Name = "DungeonMain";
            Text = "Dungeon Map";
            KeyDown += DungeonMain_KeyDown;
            pnlName.ResumeLayout(false);
            pnlName.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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