namespace MailSlots
{
    partial class frmMain
    {
        /// <summary>
        /// Требуется переменная конструктора.
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
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSend = new System.Windows.Forms.Button();
            this.tbMessage = new System.Windows.Forms.TextBox();
            this.lblMailSlot = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.tbMailSlot = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbLogin = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rtbChat = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // btnSend
            // 
            this.btnSend.Enabled = false;
            this.btnSend.Location = new System.Drawing.Point(365, 298);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(121, 32);
            this.btnSend.TabIndex = 3;
            this.btnSend.Text = "Отправить";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // tbMessage
            // 
            this.tbMessage.Location = new System.Drawing.Point(115, 303);
            this.tbMessage.Margin = new System.Windows.Forms.Padding(4);
            this.tbMessage.Name = "tbMessage";
            this.tbMessage.ReadOnly = true;
            this.tbMessage.Size = new System.Drawing.Size(240, 22);
            this.tbMessage.TabIndex = 1;
            // 
            // lblMailSlot
            // 
            this.lblMailSlot.AutoSize = true;
            this.lblMailSlot.Location = new System.Drawing.Point(16, 7);
            this.lblMailSlot.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMailSlot.Name = "lblMailSlot";
            this.lblMailSlot.Size = new System.Drawing.Size(93, 32);
            this.lblMailSlot.TabIndex = 2;
            this.lblMailSlot.Text = "Введите имя \r\nмейлслота";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(365, 43);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(121, 32);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Подключиться";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(16, 306);
            this.lblMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(81, 16);
            this.lblMessage.TabIndex = 2;
            this.lblMessage.Text = "Сообщение";
            // 
            // tbMailSlot
            // 
            this.tbMailSlot.Location = new System.Drawing.Point(115, 12);
            this.tbMailSlot.Margin = new System.Windows.Forms.Padding(4);
            this.tbMailSlot.Name = "tbMailSlot";
            this.tbMailSlot.ReadOnly = true;
            this.tbMailSlot.Size = new System.Drawing.Size(240, 22);
            this.tbMailSlot.TabIndex = 0;
            this.tbMailSlot.Text = "\\\\*\\mailslot\\ServerMailslot";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Логин";
            // 
            // tbLogin
            // 
            this.tbLogin.Location = new System.Drawing.Point(115, 48);
            this.tbLogin.Name = "tbLogin";
            this.tbLogin.Size = new System.Drawing.Size(240, 22);
            this.tbLogin.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 80);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(31, 16);
            this.label2.TabIndex = 6;
            this.label2.Text = "Чат";
            // 
            // rtbChat
            // 
            this.rtbChat.Location = new System.Drawing.Point(19, 102);
            this.rtbChat.Name = "rtbChat";
            this.rtbChat.Size = new System.Drawing.Size(467, 189);
            this.rtbChat.TabIndex = 7;
            this.rtbChat.Text = "";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 341);
            this.Controls.Add(this.rtbChat);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbLogin);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbMailSlot);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.lblMailSlot);
            this.Controls.Add(this.tbMessage);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnSend);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Клиент";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox tbMessage;
        private System.Windows.Forms.Label lblMailSlot;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Label lblMessage;
        private System.Windows.Forms.TextBox tbMailSlot;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbLogin;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtbChat;
    }
}

