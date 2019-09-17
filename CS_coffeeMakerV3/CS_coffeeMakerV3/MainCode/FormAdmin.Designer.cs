namespace CS_coffeeMakerV3
{
    partial class FormAdmin
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
            this.button_act_toggle = new System.Windows.Forms.Button();
            this.button_act_pour = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_act_toggle
            // 
            this.button_act_toggle.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_act_toggle.Location = new System.Drawing.Point(13, 63);
            this.button_act_toggle.Margin = new System.Windows.Forms.Padding(4);
            this.button_act_toggle.Name = "button_act_toggle";
            this.button_act_toggle.Size = new System.Drawing.Size(100, 42);
            this.button_act_toggle.TabIndex = 0;
            this.button_act_toggle.Text = "act_toggle";
            this.button_act_toggle.UseVisualStyleBackColor = true;
            this.button_act_toggle.Click += new System.EventHandler(this.Button_act_toggle_Click);
            // 
            // button_act_pour
            // 
            this.button_act_pour.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button_act_pour.Location = new System.Drawing.Point(13, 13);
            this.button_act_pour.Margin = new System.Windows.Forms.Padding(4);
            this.button_act_pour.Name = "button_act_pour";
            this.button_act_pour.Size = new System.Drawing.Size(100, 42);
            this.button_act_pour.TabIndex = 0;
            this.button_act_pour.Text = "act_pour";
            this.button_act_pour.UseVisualStyleBackColor = true;
            this.button_act_pour.Click += new System.EventHandler(this.Button_act_pour_Click);
            // 
            // FormAdmin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(673, 379);
            this.Controls.Add(this.button_act_pour);
            this.Controls.Add(this.button_act_toggle);
            this.Font = new System.Drawing.Font("新細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormAdmin";
            this.Text = "FormAdmin";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_act_toggle;
        private System.Windows.Forms.Button button_act_pour;
    }
}