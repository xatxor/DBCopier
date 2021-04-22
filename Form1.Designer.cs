
namespace DBCopier
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.db_open = new System.Windows.Forms.Button();
            this.db_name = new System.Windows.Forms.Label();
            this.log_textbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // db_open
            // 
            this.db_open.Location = new System.Drawing.Point(45, 38);
            this.db_open.Name = "db_open";
            this.db_open.Size = new System.Drawing.Size(99, 33);
            this.db_open.TabIndex = 0;
            this.db_open.Text = "Выбрать бд";
            this.db_open.UseVisualStyleBackColor = true;
            this.db_open.Click += new System.EventHandler(this.button1_Click);
            // 
            // db_name
            // 
            this.db_name.AutoSize = true;
            this.db_name.Location = new System.Drawing.Point(45, 78);
            this.db_name.Name = "db_name";
            this.db_name.Size = new System.Drawing.Size(190, 13);
            this.db_name.TabIndex = 1;
            this.db_name.Text = "Тут отобразится путь выбранной бд";
            // 
            // log_textbox
            // 
            this.log_textbox.Location = new System.Drawing.Point(400, 50);
            this.log_textbox.Multiline = true;
            this.log_textbox.Name = "log_textbox";
            this.log_textbox.Size = new System.Drawing.Size(356, 361);
            this.log_textbox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(116, 223);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "готово!!!";
            this.label1.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.log_textbox);
            this.Controls.Add(this.db_name);
            this.Controls.Add(this.db_open);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button db_open;
        private System.Windows.Forms.Label db_name;
        private System.Windows.Forms.TextBox log_textbox;
        private System.Windows.Forms.Label label1;
    }
}

