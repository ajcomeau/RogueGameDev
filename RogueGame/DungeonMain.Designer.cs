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
            lblStats = new Label();
            toolTip1 = new ToolTip(components);
            lblArray = new Label();
            pnlName = new Panel();
            lblPrompt = new Label();
            btnStart = new Button();
            txtName = new TextBox();
            lblQuestion = new Label();
            listStatus = new ListBox();
            pnlName.SuspendLayout();
            SuspendLayout();
            // 
            // lblStats
            // 
            lblStats.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStats.AutoSize = true;
            lblStats.Font = new Font("Consolas", 14F, FontStyle.Bold, GraphicsUnit.Point);
            lblStats.Location = new Point(13, 767);
            lblStats.Margin = new Padding(4, 0, 4, 0);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(80, 22);
            lblStats.TabIndex = 1;
            lblStats.Text = "       ";
            // 
            // lblArray
            // 
            lblArray.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblArray.Font = new Font("Consolas", 14F, FontStyle.Regular, GraphicsUnit.Point);
            lblArray.Location = new Point(14, 167);
            lblArray.Margin = new Padding(4, 0, 4, 0);
            lblArray.Name = "lblArray";
            lblArray.Size = new Size(1118, 572);
            lblArray.TabIndex = 10;
            // 
            // pnlName
            // 
            pnlName.Anchor = AnchorStyles.Bottom;
            pnlName.Controls.Add(lblPrompt);
            pnlName.Controls.Add(btnStart);
            pnlName.Controls.Add(txtName);
            pnlName.Controls.Add(lblQuestion);
            pnlName.Location = new Point(200, 717);
            pnlName.Name = "pnlName";
            pnlName.Size = new Size(727, 69);
            pnlName.TabIndex = 11;
            // 
            // lblPrompt
            // 
            lblPrompt.AutoSize = true;
            lblPrompt.Location = new Point(213, 5);
            lblPrompt.Name = "lblPrompt";
            lblPrompt.Size = new Size(264, 17);
            lblPrompt.TabIndex = 3;
            lblPrompt.Text = "Click Start to begin a new game.";
            // 
            // btnStart
            // 
            btnStart.BackColor = SystemColors.ActiveCaptionText;
            btnStart.Location = new Point(613, 31);
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
            txtName.Location = new Point(330, 32);
            txtName.Name = "txtName";
            txtName.Size = new Size(277, 23);
            txtName.TabIndex = 1;
            // 
            // lblQuestion
            // 
            lblQuestion.Location = new Point(30, 35);
            lblQuestion.Name = "lblQuestion";
            lblQuestion.Size = new Size(271, 21);
            lblQuestion.TabIndex = 0;
            lblQuestion.Text = "What is your rogue's name?";
            // 
            // listStatus
            // 
            listStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listStatus.BackColor = Color.Black;
            listStatus.BorderStyle = BorderStyle.None;
            listStatus.Font = new Font("Consolas", 12F, FontStyle.Bold, GraphicsUnit.Point);
            listStatus.ForeColor = Color.FromArgb(255, 128, 0);
            listStatus.FormattingEnabled = true;
            listStatus.HorizontalScrollbar = true;
            listStatus.ItemHeight = 19;
            listStatus.Location = new Point(12, 12);
            listStatus.Name = "listStatus";
            listStatus.Size = new Size(1120, 152);
            listStatus.TabIndex = 12;
            // 
            // DungeonMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.Black;
            ClientSize = new Size(1144, 798);
            Controls.Add(pnlName);
            Controls.Add(lblStats);
            Controls.Add(lblArray);
            Controls.Add(listStatus);
            Font = new Font("Consolas", 10F, FontStyle.Bold, GraphicsUnit.Point);
            ForeColor = Color.FromArgb(255, 128, 0);
            KeyPreview = true;
            Margin = new Padding(4);
            MinimumSize = new Size(1160, 837);
            Name = "DungeonMain";
            Text = "Dungeon Map";
            KeyDown += DungeonMain_KeyDown;
            pnlName.ResumeLayout(false);
            pnlName.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label lblStats;
        private ToolTip toolTip1;
        private Label lblArray;
        private Panel pnlName;
        private Button btnStart;
        private TextBox txtName;
        private Label lblQuestion;
        private ListBox listStatus;
        private Label lblPrompt;
    }
}