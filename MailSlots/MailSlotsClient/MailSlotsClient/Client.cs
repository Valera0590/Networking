using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace MailSlots
{
    public partial class frmMain : Form
    {
        private Int32 ServerHandleMailSlot;   // дескриптор мэйлслота
        private Int32 ClientHandleMailSlot;
        private Thread t;                       // поток для обслуживания мэйлслота
        private bool _continue = true;          // флаг, указывающий продолжается ли работа с мэйлслотом

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();
            this.Text += "     " + Dns.GetHostName();   // выводим имя текущей машины в заголовок формы
        }

        // присоединение к мэйлслоту
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // открываем мэйлслот, имя которого ServerMailSlot
                ServerHandleMailSlot = DIS.Import.CreateFile(tbMailSlot.Text, DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
                if (ServerHandleMailSlot != -1)
                {
                    tbLogin.Enabled = false;
                    tbMailSlot.Enabled = false;
                    btnSend.Enabled = true;
                    btnConnect.Enabled = false;
                    tbMessage.ReadOnly = false;
                    if (tbLogin.Text == "") tbLogin.Text = Dns.GetHostName().ToString();    // Если клиент анонимный 
                    else this.Text = "Клиент     " + tbLogin.Text;   // выводим имя текущего пользователя в заголовок формы
                    uint BytesWritten = 0;  // количество реально записанных в мэйлслот байт
                    byte[] buff = Encoding.Unicode.GetBytes(tbLogin.Text + " > присоединился к чату");    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
                    DIS.Import.WriteFile(ServerHandleMailSlot, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);     // выполняем запись последовательности байт в мэйлслот

                    // создание мэйлслота
                    ClientHandleMailSlot = DIS.Import.CreateMailslot("\\\\.\\mailslot\\ClientMailslot"+tbLogin.Text, 0, DIS.Types.MAILSLOT_WAIT_FOREVER, 0);
                     // создание потока, отвечающего за работу с мэйлслотом
                    Thread t = new Thread(ReceiveMessage);
                    t.Start();
                }
                else
                    MessageBox.Show("Не удалось подключиться к мейлслоту");
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к мейлслоту");
            }
        }

        // отправка сообщения
        private void btnSend_Click(object sender, EventArgs e)
        {
            uint BytesWritten = 0;  // количество реально записанных в мэйлслот байт
            byte[] buff = Encoding.Unicode.GetBytes(tbLogin.Text + " > " + tbMessage.Text);    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
            DIS.Import.WriteFile(ServerHandleMailSlot, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);     // выполняем запись последовательности байт в мэйлслот
            tbMessage.Text = "";
        }

        private void ReceiveMessage()
        {
            string msg = "";            // прочитанное сообщение
            int MailslotSize = 0;       // максимальный размер сообщения
            int lpNextSize = 0;         // размер следующего сообщения
            int MessageCount = 0;       // количество сообщений в мэйлслоте
            uint realBytesReaded = 0;   // количество реально прочитанных из мэйлслота байтов

            // входим в бесконечный цикл работы с мэйлслотом
            while (_continue)
            {
                // получаем информацию о состоянии мэйлслота
                DIS.Import.GetMailslotInfo(ClientHandleMailSlot, MailslotSize, ref lpNextSize, ref MessageCount, 0);

                // если есть сообщения в мэйлслоте, то обрабатываем каждое из них
                if (MessageCount > 0)
                    for (int i = 0; i < MessageCount; i++)
                    {
                        byte[] buff = new byte[1024];                           // буфер прочитанных из мэйлслота байтов
                        DIS.Import.FlushFileBuffers(ClientHandleMailSlot);      // "принудительная" запись данных, расположенные в буфере операционной системы, в файл мэйлслота
                        DIS.Import.ReadFile(ClientHandleMailSlot, buff, 1024, ref realBytesReaded, 0);      // считываем последовательность байтов из мэйлслота в буфер buff
                        msg = Encoding.Unicode.GetString(buff);                 // выполняем преобразование байтов в последовательность символов

                        rtbChat.Invoke((MethodInvoker)delegate
                        {
                            if (msg != "")
                                rtbChat.Text += "\n" + msg;     // выводим полученное сообщение на форму
                        });
                        Thread.Sleep(500);                                      // приостанавливаем работу потока перед тем, как приcтупить к обслуживанию очередного клиента
                        //DIS.Import.CloseHandle(ServerHandleMailSlot);            // отключаемся от мэйлслота сервера
                    }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (tbLogin.Text != "")
            {
                uint BytesWritten = 0;  // количество реально записанных в мэйлслот байт
                byte[] buff = Encoding.Unicode.GetBytes(tbLogin.Text + " > вышел из чата");    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
                DIS.Import.WriteFile(ServerHandleMailSlot, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);     // выполняем запись последовательности байт в мэйлслот
            }
            DIS.Import.CloseHandle(ServerHandleMailSlot);     // закрываем дескриптор мэйлслота
            _continue = false;      // сообщаем, что работа с мэйлслотом завершена

            if (t != null)
                t.Abort();          // завершаем поток

            if (ClientHandleMailSlot != -1)
                DIS.Import.CloseHandle(ClientHandleMailSlot);            // закрываем дескриптор мэйлслота
        }

        //private void tbLogin_TextChanged(object sender, EventArgs e)
        //{
        //    if (tbLogin.Text != "") btnConnect.Enabled = true;
        //    else btnConnect.Enabled = false;
        //}
    }
}