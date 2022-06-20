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
using System.Runtime.InteropServices;

namespace MailSlots
{
    public partial class frmMain : Form
    {
        private int ServerHandleMailSlot;       // дескриптор мэйлслота
        private int ClientsHandleMailSlot;
        private string clientName = "\\\\.\\mailslot\\ClientMailslot";

        private string MailSlotName = "\\\\" + Dns.GetHostName() + "\\mailslot\\ServerMailslot";    // имя мэйлслота, Dns.GetHostName() - метод, возвращающий имя машины, на которой запущено приложение
        private Thread t;                       // поток для обслуживания мэйлслота
        private bool _continue = true;          // флаг, указывающий продолжается ли работа с мэйлслотом
        private List<string> logins = new List<string>();
        private List<string> clients = new List<string>();

        public int CountList(List<string> strs)
        {
            int i = 0;
            foreach (var list in strs)
                i++;
            return i;
        }

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();

            // создание мэйлслота
            ServerHandleMailSlot = DIS.Import.CreateMailslot("\\\\.\\mailslot\\ServerMailslot", 0, DIS.Types.MAILSLOT_WAIT_FOREVER, 0);

            // вывод имени мэйлслота в заголовок формы, чтобы можно было его использовать для ввода имени в форме клиента, запущенного на другом вычислительном узле
            this.Text += "     " + MailSlotName;

            // создание потока, отвечающего за работу с мэйлслотом
            Thread t = new Thread(ReceiveMessage);
            t.Start();
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
                DIS.Import.GetMailslotInfo(ServerHandleMailSlot, MailslotSize, ref lpNextSize, ref MessageCount, 0);

                // если есть сообщения в мэйлслоте, то обрабатываем каждое из них
                if (MessageCount > 0)
                    for (int i = 0; i < MessageCount; i++)
                    {
                        byte[] buff = new byte[1024];                           // буфер прочитанных из мэйлслота байтов
                        DIS.Import.FlushFileBuffers(ServerHandleMailSlot);      // "принудительная" запись данных, расположенные в буфере операционной системы, в файл мэйлслота
                        DIS.Import.ReadFile(ServerHandleMailSlot, buff, 1024, ref realBytesReaded, 0);      // считываем последовательность байтов из мэйлслота в буфер buff
                        msg = Encoding.Unicode.GetString(buff);                 // выполняем преобразование байтов в последовательность символов
                        
                        rtbMessages.Invoke((MethodInvoker)delegate
                        {
                            if (msg != "")
                                rtbMessages.Text += "\n" + msg;     // выводим полученное сообщение на форму
                        });
                        string[] vs = msg.Split('>');
                        if (!logins.Contains(vs[0])) logins.Add(vs[0]);
                        if (!clients.Contains(clientName + vs[0])) clients.Add(clientName + vs[0]);
                        buff = Encoding.Unicode.GetBytes(msg);    // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
                        int j = 0;
                        foreach (var item in logins)    //отправка сообщения всем клиентам
                        {
                            uint BytesWritten = 0;  // количество реально записанных в мэйлслот байт
                            // открываем мэйлслот, имя которого ServerMailSlot
                            ClientsHandleMailSlot = DIS.Import.CreateFile(clients[j], DIS.Types.EFileAccess.GenericWrite, DIS.Types.EFileShare.Read, 0, DIS.Types.ECreationDisposition.OpenExisting, 0, 0);

                            DIS.Import.WriteFile(ClientsHandleMailSlot, buff, Convert.ToUInt32(buff.Length), ref BytesWritten, 0);
                            Thread.Sleep(10);
                            j++;
                        }
                        if (vs.Length > 1 && vs[1].Remove(14) == " вышел из чата")
                        {
                            logins.Remove(vs[0]);
                            clients.Remove(clientName + vs[0]);
                        }
                        Thread.Sleep(500);                                      // приостанавливаем работу потока перед тем, как приcтупить к обслуживанию очередного клиента
                    }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;      // сообщаем, что работа с мэйлслотом завершена

            if (t != null)
                t.Abort();          // завершаем поток

            if (ServerHandleMailSlot != -1)
                DIS.Import.CloseHandle(ServerHandleMailSlot);            // закрываем дескриптор мэйлслота
        }
    }
}