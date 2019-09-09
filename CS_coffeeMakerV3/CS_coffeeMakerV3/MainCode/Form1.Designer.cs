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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            this.listView1 = new System.Windows.Forms.ListView();
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
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_RS)).BeginInit();
            this.contextMenuStrip_imageBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView1.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.listView1.Location = new System.Drawing.Point(16, 17);
            this.listView1.Margin = new System.Windows.Forms.Padding(4);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(186, 708);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
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
            // 
            // contextMenuStrip_imageBox
            // 
            this.contextMenuStrip_imageBox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.realSenseToolStripMenuItem});
            this.contextMenuStrip_imageBox.Name = "contextMenuStrip1";
            this.contextMenuStrip_imageBox.Size = new System.Drawing.Size(181, 48);
            // 
            // realSenseToolStripMenuItem
            // 
            this.realSenseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorToolStripMenuItem,
            this.depthToolStripMenuItem,
            this.mixToolStripMenuItem,
            this.toolStripSeparator1,
            this.stopToolStripMenuItem});
            this.realSenseToolStripMenuItem.Name = "realSenseToolStripMenuItem";
            this.realSenseToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.realSenseToolStripMenuItem.Text = "Real Sense";
            // 
            // colorToolStripMenuItem
            // 
            this.colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            this.colorToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.colorToolStripMenuItem.Text = "Color";
            this.colorToolStripMenuItem.Click += new System.EventHandler(this.ColorToolStripMenuItem_Click);
            // 
            // depthToolStripMenuItem
            // 
            this.depthToolStripMenuItem.Name = "depthToolStripMenuItem";
            this.depthToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.depthToolStripMenuItem.Text = "Depth";
            // 
            // mixToolStripMenuItem
            // 
            this.mixToolStripMenuItem.Name = "mixToolStripMenuItem";
            this.mixToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.mixToolStripMenuItem.Text = "Mix";
            this.mixToolStripMenuItem.Click += new System.EventHandler(this.MixToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // stopToolStripMenuItem
            // 
            this.stopToolStripMenuItem.Name = "stopToolStripMenuItem";
            this.stopToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.stopToolStripMenuItem.Text = "Stop";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1342, 758);
            this.Controls.Add(this.imageBox_RS);
            this.Controls.Add(this.listView1);
            this.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_RS)).EndInit();
            this.contextMenuStrip_imageBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
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
    }
}

