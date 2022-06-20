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

namespace Pipes
{
    public partial class frmMain : Form
    {
        private Int32 PipeHandle;   // дескриптор канала
        private Int32 PipeHandle2;
        private string PipeName = "\\\\.\\pipe\\ServerPipe";    // имя канала, Dns.GetHostName() - метод, возвращающий имя машины, на которой запущено приложение
        private string clName = "\\\\.\\pipe\\ClientPipe";
        private Thread t;                                                               // поток для обслуживания канала
        private bool _continue = true;                                                  // флаг, указывающий продолжается ли работа с каналом

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();
            tbMessage.Enabled = false;
            
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            uint BytesWritten = 0;  // количество реально записанных в канал байт
            byte[] buff = Encoding.Unicode.GetBytes(tbLogin.Text + " > " + tbMessage.Text);    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт

            // открываем именованный канал, имя которого указано в поле tbPipe
            PipeHandle = DIS.Import.CreateFile(PipeName, DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
            DIS.Import.WriteFile(PipeHandle, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);         // выполняем запись последовательности байт в канал
            DIS.Import.CloseHandle(PipeHandle);                                                                 // закрываем дескриптор канала
            tbMessage.Text = "";
        }

        private void ReceiveMessage()
        {
            string msg = "";            // прочитанное сообщение
            uint realBytesReaded = 0;   // количество реально прочитанных из канала байтов
            // входим в бесконечный цикл работы с каналом
            while (_continue)
            {
                if (DIS.Import.ConnectNamedPipe(PipeHandle2, 0))
                {
                    byte[] buff = new byte[1024];                                           // буфер прочитанных из канала байтов
                    DIS.Import.FlushFileBuffers(PipeHandle2);                                // "принудительная" запись данных, расположенные в буфере операционной системы, в файл именованного канала
                    DIS.Import.ReadFile(PipeHandle2, buff, 1024, ref realBytesReaded, 0);    // считываем последовательность байтов из канала в буфер buff
                    msg = Encoding.Unicode.GetString(buff);                                 // выполняем преобразование байтов в последовательность символов
                    rtbChat.Invoke((MethodInvoker)delegate
                    {
                        if (msg != "")
                        {
                            rtbChat.Text += "\n" + msg;                             // выводим полученное сообщение на форму
                        }
                    });

                    DIS.Import.DisconnectNamedPipe(PipeHandle2);                             // отключаемся от канала клиента 
                    Thread.Sleep(500);                                                      // приостанавливаем работу потока перед тем, как приcтупить к обслуживанию очередного клиента
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)  //Присоединение клиента к серверу
        {
            tbMessage.Enabled = true;
            tbLogin.Enabled = false;
            btnConnect.Enabled = false;
            this.Text += "     " + tbLogin.Text;   // выводим имя текущей машины в заголовок формы
            uint BytesWritten = 0;  // количество реально записанных в канал байт
            byte[] buff = Encoding.Unicode.GetBytes(tbLogin.Text + " > присоединился к чату");    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт

            // открываем именованный канал, имя которого указано в поле tbPipe
            PipeHandle = DIS.Import.CreateFile(PipeName, DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
            DIS.Import.WriteFile(PipeHandle, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);         // выполняем запись последовательности байт в канал
            DIS.Import.CloseHandle(PipeHandle);
            PipeHandle2 = DIS.Import.CreateNamedPipe(clName + tbLogin.Text, DIS.Types.PIPE_ACCESS_DUPLEX, DIS.Types.PIPE_TYPE_BYTE | DIS.Types.PIPE_WAIT, DIS.Types.PIPE_UNLIMITED_INSTANCES, 0, 1024, DIS.Types.NMPWAIT_WAIT_FOREVER, (uint)0);

            t = new Thread(ReceiveMessage);
            t.Start();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            uint BytesWritten = 0;  // количество реально записанных в канал байт
            byte[] buff;
            if (tbLogin.Text != "")
            {
                buff = Encoding.Unicode.GetBytes(tbLogin.Text + " > вышел из чата");    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт

                // открываем именованный канал, имя которого указано в поле tbPipe
                PipeHandle = DIS.Import.CreateFile(PipeName, DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
                DIS.Import.WriteFile(PipeHandle, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);         // выполняем запись последовательности байт в канал
                DIS.Import.CloseHandle(PipeHandle);
            }
            _continue = false;      // сообщаем, что работа с каналом завершена

            if (t != null)
                t.Abort();          // завершаем поток

            if (PipeHandle2 != -1)
                DIS.Import.CloseHandle(PipeHandle2);     // закрываем дескриптор канала
        }
    }
}
