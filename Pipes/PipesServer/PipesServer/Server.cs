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
using System.Threading;

namespace Pipes
{
    public partial class frmMain : Form
    {
        private Int32 PipeHandle;                                                       // дескриптор канала
        private Int32 PipeHandle2;
        private string PipeName = "\\\\.\\pipe\\ServerPipe";    // имя канала, Dns.GetHostName() - метод, возвращающий имя машины, на которой запущено приложение
        private string clientName = "\\\\.\\pipe\\ClientPipe";
        private Thread t;                                                               // поток для обслуживания канала
        private bool _continue = true;                                                  // флаг, указывающий продолжается ли работа с каналом
        private List<string> logins = new List<string>();
        private List<string> clients = new List<string>();
        /*public void Add(string item)
        {
            // Проверяем входные данные на пустоту.
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            // Множество может содержать только уникальные элементы,
            // поэтому если множество уже содержит такой элемент данных, то не добавляем его.
            if (!logins.Contains(item))
            {
                logins.Add(item);
            }
        }*/

        public int CountList(List<string> strs)
        {
            int i=0;
            foreach(var list in strs)
                i++;
            return i;
        }
        // конструктор формы
        public frmMain()
        {
            InitializeComponent();

            // создание именованного канала
            PipeHandle = DIS.Import.CreateNamedPipe(PipeName, DIS.Types.PIPE_ACCESS_DUPLEX, DIS.Types.PIPE_TYPE_BYTE | DIS.Types.PIPE_WAIT, DIS.Types.PIPE_UNLIMITED_INSTANCES, 0, 1024, DIS.Types.NMPWAIT_WAIT_FOREVER, (uint)0);
            
            // вывод имени канала в заголовок формы, чтобы можно было его использовать для ввода имени в форме клиента, запущенного на другом вычислительном узле
            this.Text += "     " + PipeName;
            
            // создание потока, отвечающего за работу с каналом
            t = new Thread(ReceiveMessage);
            t.Start();
        }

        private void ReceiveMessage()
        {
            string msg = "";            // прочитанное сообщение
            uint realBytesReaded = 0;   // количество реально прочитанных из канала байтов
            // входим в бесконечный цикл работы с каналом
            while (_continue)
            {
                if (DIS.Import.ConnectNamedPipe(PipeHandle, 0))
                {
                    byte[] buff = new byte[1024];                                           // буфер прочитанных из канала байтов
                    DIS.Import.FlushFileBuffers(PipeHandle);                                // "принудительная" запись данных, расположенные в буфере операционной системы, в файл именованного канала
                    DIS.Import.ReadFile(PipeHandle, buff, 1024, ref realBytesReaded, 0);    // считываем последовательность байтов из канала в буфер buff
                    msg = Encoding.Unicode.GetString(buff);                                 // выполняем преобразование байтов в последовательность символов
                    rtbMessages.Invoke((MethodInvoker)delegate
                    {
                        if (msg != "")
                        {
                            rtbMessages.Text += "\n" + msg;                             // выводим полученное сообщение на форму
                            string[] vs = msg.Split('>'); 
                            if (!logins.Contains(vs[0])) logins.Add(vs[0]);
                            if (!clients.Contains(clientName + vs[0])) clients.Add(clientName + vs[0]);
                            //rtbMessages.Text += "\nКол-во логинов - " + CountList(logins) + "\nКол-во клиентов - " + CountList(clients);
                            buff = Encoding.Unicode.GetBytes(msg);
                            int i = 0;
                            foreach (var item in logins)    //отправка сообщения всем клиентам
                            {
                                uint BytesWritten = 0;
                                // открываем именованный канал, имя которого указано в поле tbPipe
                                PipeHandle2 = DIS.Import.CreateFile(clients[i], DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);
                                DIS.Import.WriteFile(PipeHandle2, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);         // выполняем запись последовательности байт в канал
                                DIS.Import.CloseHandle(PipeHandle2);
                                Thread.Sleep(10);
                                i++;
                            }
                            if (vs.Length > 1 && vs[1].Remove(14) == " вышел из чата")
                            {
                                logins.Remove(vs[0]);
                                clients.Remove(clientName + vs[0]);
                            }
                        }
                    });

                    DIS.Import.DisconnectNamedPipe(PipeHandle);                             // отключаемся от канала клиента 
                    Thread.Sleep(500);                                                      // приостанавливаем работу потока перед тем, как приcтупить к обслуживанию очередного клиента
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;      // сообщаем, что работа с каналом завершена

            if (t != null)
                t.Abort();          // завершаем поток
            
            if (PipeHandle != -1)
                DIS.Import.CloseHandle(PipeHandle);     // закрываем дескриптор канала
        }

        private void rtbMessages_TextChanged(object sender, EventArgs e)
        {

        }
    }
}