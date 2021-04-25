
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
            this.components = new System.ComponentModel.Container();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.db_open = new System.Windows.Forms.Button();
            this.log_textbox = new System.Windows.Forms.TextBox();
            this.date_textbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.start_button = new System.Windows.Forms.Button();
            this.path_label = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer_label = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // db_open
            // 
            this.db_open.Location = new System.Drawing.Point(27, 26);
            this.db_open.Name = "db_open";
            this.db_open.Size = new System.Drawing.Size(118, 35);
            this.db_open.TabIndex = 0;
            this.db_open.Text = "Выбрать бд";
            this.db_open.UseVisualStyleBackColor = true;
            this.db_open.Click += new System.EventHandler(this.db_open_Click);
            // 
            // log_textbox
            // 
            this.log_textbox.Location = new System.Drawing.Point(27, 148);
            this.log_textbox.Multiline = true;
            this.log_textbox.Name = "log_textbox";
            this.log_textbox.ReadOnly = true;
            this.log_textbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.log_textbox.Size = new System.Drawing.Size(330, 185);
            this.log_textbox.TabIndex = 2;
            // 
            // date_textbox
            // 
            this.date_textbox.Location = new System.Drawing.Point(27, 112);
            this.date_textbox.Name = "date_textbox";
            this.date_textbox.Size = new System.Drawing.Size(183, 20);
            this.date_textbox.TabIndex = 3;
            this.date_textbox.Text = "01-01-2021";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 91);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(188, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Введите дату в формате dd-mm-yyyy";
            // 
            // start_button
            // 
            this.start_button.Location = new System.Drawing.Point(239, 97);
            this.start_button.Name = "start_button";
            this.start_button.Size = new System.Drawing.Size(118, 35);
            this.start_button.TabIndex = 5;
            this.start_button.Text = "Начать";
            this.start_button.UseVisualStyleBackColor = true;
            this.start_button.Click += new System.EventHandler(this.start_button_Click);
            // 
            // path_label
            // 
            this.path_label.AutoSize = true;
            this.path_label.Location = new System.Drawing.Point(24, 68);
            this.path_label.Name = "path_label";
            this.path_label.Size = new System.Drawing.Size(181, 13);
            this.path_label.TabIndex = 6;
            this.path_label.Text = "Тут отобразится полный путь к бд";
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer_label
            // 
            this.timer_label.AutoSize = true;
            this.timer_label.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.timer_label.Location = new System.Drawing.Point(278, 32);
            this.timer_label.Name = "timer_label";
            this.timer_label.Size = new System.Drawing.Size(49, 20);
            this.timer_label.TabIndex = 7;
            this.timer_label.Text = "00:00";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 361);
            this.Controls.Add(this.timer_label);
            this.Controls.Add(this.path_label);
            this.Controls.Add(this.start_button);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.date_textbox);
            this.Controls.Add(this.log_textbox);
            this.Controls.Add(this.db_open);
            this.Name = "Form1";
            this.Text = "DBCopier";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button db_open;
        private System.Windows.Forms.TextBox log_textbox;
        private System.Windows.Forms.TextBox date_textbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button start_button;
        private System.Windows.Forms.Label path_label;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label timer_label;
    }
}

