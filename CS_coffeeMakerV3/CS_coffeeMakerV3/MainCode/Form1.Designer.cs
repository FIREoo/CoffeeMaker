namespace CS_coffeeMakerV3
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("");
            this.listView_action = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageBox_RS = new Emgu.CV.UI.ImageBox();
            this.contextMenuStrip_imageBox = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.realSenseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.depthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mixToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.stopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label_Bcup_msg = new System.Windows.Forms.Label();
            this.label_Pcup_msg = new System.Windows.Forms.Label();
            this.label_Bcup_state = new System.Windows.Forms.Label();
            this.label_Pcup_state = new System.Windows.Forms.Label();
            this.imageBox_Bcup = new Emgu.CV.UI.ImageBox();
            this.imageBox_Pcup = new Emgu.CV.UI.ImageBox();
            this.button_startRecordState = new System.Windows.Forms.Button();
            this.button_showAdmin = new System.Windows.Forms.Button();
            this.pCMixToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_RS)).BeginInit();
            this.contextMenuStrip_imageBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_Bcup)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_Pcup)).BeginInit();
            this.SuspendLayout();
            // 
            // listView_action
            // 
            this.listView_action.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView_action.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.listView_action.GridLines = true;
            this.listView_action.HideSelection = false;
            this.listView_action.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem5});
            this.listView_action.Location = new System.Drawing.Point(16, 17);
            this.listView_action.Margin = new System.Windows.Forms.Padding(4);
            this.listView_action.Name = "listView_action";
            this.listView_action.Size = new System.Drawing.Size(186, 708);
            this.listView_action.TabIndex = 0;
            this.listView_action.UseCompatibleStateImageBehavior = false;
            this.listView_action.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 5;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Action";
            this.columnHeader2.Width = 81;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Detial";
            this.columnHeader3.Width = 87;
            // 
            // imageBox_RS
            // 
            this.imageBox_RS.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.imageBox_RS.ContextMenuStrip = this.contextMenuStrip_imageBox;
            this.imageBox_RS.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            this.imageBox_RS.Location = new System.Drawing.Point(209, 17);
            this.imageBox_RS.Name = "imageBox_RS";
            this.imageBox_RS.Size = new System.Drawing.Size(640, 360);
            this.imageBox_RS.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox_RS.TabIndex = 2;
            this.imageBox_RS.TabStop = false;
            this.imageBox_RS.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ImageBox_RS_MouseDown);
            // 
            // contextMenuStrip_imageBox
            // 
            this.contextMenuStrip_imageBox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.realSenseToolStripMenuItem});
            this.contextMenuStrip_imageBox.Name = "contextMenuStrip1";
            this.contextMenuStrip_imageBox.Size = new System.Drawing.Size(140, 26);
            // 
            // realSenseToolStripMenuItem
            // 
            this.realSenseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorToolStripMenuItem,
            this.depthToolStripMenuItem,
            this.mixToolStripMenuItem,
            this.pCMixToolStripMenuItem,
            this.toolStripSeparator1,
            this.stopToolStripMenuItem});
            this.realSenseToolStripMenuItem.Name = "realSenseToolStripMenuItem";
            this.realSenseToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.realSenseToolStripMenuItem.Text = "Real Sense";
            // 
            // colorToolStripMenuItem
            // 
            this.colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            this.colorToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.colorToolStripMenuItem.Text = "Color";
            this.colorToolStripMenuItem.Click += new System.EventHandler(this.ColorToolStripMenuItem_Click);
            // 
            // depthToolStripMenuItem
            // 
            this.depthToolStripMenuItem.Name = "depthToolStripMenuItem";
            this.depthToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.depthToolStripMenuItem.Text = "Depth";
            // 
            // mixToolStripMenuItem
            // 
            this.mixToolStripMenuItem.Name = "mixToolStripMenuItem";
            this.mixToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.mixToolStripMenuItem.Text = "Mix";
            this.mixToolStripMenuItem.Click += new System.EventHandler(this.MixToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(111, 6);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            // 
            // imageBox1
            // 
            this.imageBox1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.imageBox1.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            this.imageBox1.Location = new System.Drawing.Point(209, 383);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(640, 220);
            this.imageBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox1.TabIndex = 2;
            this.imageBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(855, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "blue cup";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(855, 122);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 24);
            this.label2.TabIndex = 3;
            this.label2.Text = "pink cup";
            // 
            // label_Bcup_msg
            // 
            this.label_Bcup_msg.AutoSize = true;
            this.label_Bcup_msg.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Bcup_msg.Location = new System.Drawing.Point(855, 80);
            this.label_Bcup_msg.Name = "label_Bcup_msg";
            this.label_Bcup_msg.Size = new System.Drawing.Size(63, 24);
            this.label_Bcup_msg.TabIndex = 3;
            this.label_Bcup_msg.Text = "(0,0,0)";
            // 
            // label_Pcup_msg
            // 
            this.label_Pcup_msg.AutoSize = true;
            this.label_Pcup_msg.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Pcup_msg.Location = new System.Drawing.Point(855, 171);
            this.label_Pcup_msg.Name = "label_Pcup_msg";
            this.label_Pcup_msg.Size = new System.Drawing.Size(63, 24);
            this.label_Pcup_msg.TabIndex = 3;
            this.label_Pcup_msg.Text = "(0,0,0)";
            // 
            // label_Bcup_state
            // 
            this.label_Bcup_state.AutoSize = true;
            this.label_Bcup_state.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Bcup_state.Location = new System.Drawing.Point(855, 56);
            this.label_Bcup_state.Name = "label_Bcup_state";
            this.label_Bcup_state.Size = new System.Drawing.Size(66, 24);
            this.label_Bcup_state.TabIndex = 3;
            this.label_Bcup_state.Text = "[state]";
            // 
            // label_Pcup_state
            // 
            this.label_Pcup_state.AutoSize = true;
            this.label_Pcup_state.Font = new System.Drawing.Font("微軟正黑體", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_Pcup_state.Location = new System.Drawing.Point(855, 147);
            this.label_Pcup_state.Name = "label_Pcup_state";
            this.label_Pcup_state.Size = new System.Drawing.Size(66, 24);
            this.label_Pcup_state.TabIndex = 3;
            this.label_Pcup_state.Text = "[state]";
            // 
            // imageBox_Bcup
            // 
            this.imageBox_Bcup.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.imageBox_Bcup.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            this.imageBox_Bcup.Location = new System.Drawing.Point(969, 17);
            this.imageBox_Bcup.Name = "imageBox_Bcup";
            this.imageBox_Bcup.Size = new System.Drawing.Size(300, 100);
            this.imageBox_Bcup.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox_Bcup.TabIndex = 2;
            this.imageBox_Bcup.TabStop = false;
            // 
            // imageBox_Pcup
            // 
            this.imageBox_Pcup.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.imageBox_Pcup.FunctionalMode = Emgu.CV.UI.ImageBox.FunctionalModeOption.Minimum;
            this.imageBox_Pcup.Location = new System.Drawing.Point(969, 123);
            this.imageBox_Pcup.Name = "imageBox_Pcup";
            this.imageBox_Pcup.Size = new System.Drawing.Size(300, 100);
            this.imageBox_Pcup.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox_Pcup.TabIndex = 2;
            this.imageBox_Pcup.TabStop = false;
            // 
            // button_startRecordState
            // 
            this.button_startRecordState.Location = new System.Drawing.Point(969, 229);
            this.button_startRecordState.Name = "button_startRecordState";
            this.button_startRecordState.Size = new System.Drawing.Size(75, 35);
            this.button_startRecordState.TabIndex = 4;
            this.button_startRecordState.Text = "button1";
            this.button_startRecordState.UseVisualStyleBackColor = true;
            this.button_startRecordState.Click += new System.EventHandler(this.Button_startRecordState_Click);
            // 
            // button_showAdmin
            // 
            this.button_showAdmin.Location = new System.Drawing.Point(1218, 695);
            this.button_showAdmin.Name = "button_showAdmin";
            this.button_showAdmin.Size = new System.Drawing.Size(112, 51);
            this.button_showAdmin.TabIndex = 5;
            this.button_showAdmin.Text = "Admin";
            this.button_showAdmin.UseVisualStyleBackColor = true;
            this.button_showAdmin.Click += new System.EventHandler(this.Button_showAdmin_Click);
            // 
            // pCMixToolStripMenuItem
            // 
            this.pCMixToolStripMenuItem.Name = "pCMixToolStripMenuItem";
            this.pCMixToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.pCMixToolStripMenuItem.Text = "PC Mix";
            this.pCMixToolStripMenuItem.Click += new System.EventHandler(this.PCMixToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1342, 758);
            this.Controls.Add(this.button_showAdmin);
            this.Controls.Add(this.button_startRecordState);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label_Pcup_msg);
            this.Controls.Add(this.label_Pcup_state);
            this.Controls.Add(this.label_Bcup_state);
            this.Controls.Add(this.label_Bcup_msg);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.imageBox_Pcup);
            this.Controls.Add(this.imageBox_Bcup);
            this.Controls.Add(this.imageBox1);
            this.Controls.Add(this.imageBox_RS);
            this.Controls.Add(this.listView_action);
            this.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_RS)).EndInit();
            this.contextMenuStrip_imageBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_Bcup)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_Pcup)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView_action;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private robotTool robotTool1;
        private Emgu.CV.UI.ImageBox imageBox_RS;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_imageBox;
        private System.Windows.Forms.ToolStripMenuItem realSenseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem depthToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mixToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem stopToolStripMenuItem;
        private Emgu.CV.UI.ImageBox imageBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_Bcup_msg;
        private System.Windows.Forms.Label label_Pcup_msg;
        private System.Windows.Forms.Label label_Bcup_state;
        private System.Windows.Forms.Label label_Pcup_state;
        private Emgu.CV.UI.ImageBox imageBox_Bcup;
        private Emgu.CV.UI.ImageBox imageBox_Pcup;
        private System.Windows.Forms.Button button_startRecordState;
        private System.Windows.Forms.Button button_showAdmin;
        private System.Windows.Forms.ToolStripMenuItem pCMixToolStripMenuItem;
    }
}

