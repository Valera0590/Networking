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

namespace Sockets
{
    public partial class frmMain : Form
    {
        private TcpClient Client = new TcpClient();     // клиентский сокет
        private IPAddress IP;                           // IP-адрес клиента
        private Socket ServerSock;                      // серверный сокет
        private TcpListener Listener;                   // сокет клиента
        private List<Thread> Threads = new List<Thread>();      // список потоков приложения (кроме родительского)
        private bool _continue = true;                          // флаг, указывающий продолжается ли работа с сокетами

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());    // информация об IP-адресах и имени машины, на которой запущено приложение
            IP = hostEntry.AddressList[0];                                  // IP-адрес, который будет указан в заголовке окна для идентификации клиента

            // определяем IP-адрес машины в формате IPv4
            foreach (IPAddress address in hostEntry.AddressList)
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP = address;
                    break;
                }

            this.Text += "     " + IP.ToString();                           // выводим IP-адрес текущей машины в заголовок формы
        }

        // подключение к серверному сокету
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                int Port = 1010;                                // номер порта, через который выполняется обмен сообщениями
                IPAddress IPServ = IPAddress.Parse(tbIP.Text);      // разбор IP-адреса сервера, указанного в поле tbIP
                Client.Connect(IPServ, Port);                       // подключение к серверному сокету
                tbIP.ReadOnly = true;
                tbLogin.ReadOnly = true;
                btnConnect.Enabled = false;
                btnSend.Enabled = true;
                this.Text += "     " + tbLogin.Text;
                Random rnd = new Random(DateTime.Now.Millisecond);
                int PortClient = rnd.Next(10, 1010);
                //rtbChat.Text += PortClient;
                byte[] buff = Encoding.Unicode.GetBytes(PortClient.ToString()+"!"+IP.ToString() + ">>" + tbLogin.Text + ">>" + "присоединился(-ась) к чату");   // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
                Stream stm = Client.GetStream();                                                    // получаем файловый поток клиентского сокета
                stm.Write(buff, 0, buff.Length);                                                    // выполняем запись последовательности байт
                                                                                                    // создаем серверный сокет (Listener для приема заявок от клиентских сокетов)
                Listener = new TcpListener(IP, PortClient);
                Listener.Start();

                // создаем и запускаем поток, выполняющий обслуживание серверного сокета
                Threads.Clear();
                Threads.Add(new Thread(ReceiveMessage));
                Threads[Threads.Count - 1].Start();
            }
            catch
            {
                MessageBox.Show("Введен некорректный IP-адрес");
                tbIP.ReadOnly = false;
                tbLogin.ReadOnly = false;
                btnConnect.Enabled = true;
                btnSend.Enabled = false;
            }
        }

        // работа с серверным сокетом
        private void ReceiveMessage()
        {
            // входим в бесконечный цикл для работы с серверным сокетом
            while (_continue)
            {
                ServerSock = Listener.AcceptSocket();           // получаем ссылку на очередной клиентский сокет
                Threads.Add(new Thread(ReadMessages));          // создаем и запускаем поток, обслуживающий конкретный клиентский сокет
                Threads[Threads.Count - 1].Start(ServerSock);
            }
        }

        // получение сообщений от сервера
        private void ReadMessages(object ServerSock)
        {
            string msg = "";        // полученное сообщение

            // входим в бесконечный цикл для работы с серверным сокетом
            while (_continue)
            {
                byte[] buff = new byte[1024];                           // буфер прочитанных из сокета байтов
                ((Socket)ServerSock).Receive(buff);                     // получаем последовательность байтов из сокета в буфер buff
                msg = System.Text.Encoding.Unicode.GetString(buff);     // выполняем преобразование байтов в последовательность символов
                msg = msg.Replace("\0", "");
                if (msg != "")
                {
                    rtbChat.Invoke((MethodInvoker)delegate
                    {
                        rtbChat.Text += "\n" + msg;             // выводим полученное сообщение на форму
                    });
                }
                Thread.Sleep(500);
            }
        }

        // отправка сообщения
        private void btnSend_Click(object sender, EventArgs e)
        {
            byte[] buff = Encoding.Unicode.GetBytes(IP.ToString() + ">>" + tbLogin.Text + ">>" + tbMessage.Text);   // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
            Stream stm = Client.GetStream();                                                    // получаем файловый поток клиентского сокета
            stm.Write(buff, 0, buff.Length);                                                    // выполняем запись последовательности байт
            tbMessage.Text = "";
        }


        private void tbLogin_TextChanged(object sender, EventArgs e)
        {
            if (tbIP.Text != "" && tbLogin.Text != "")
                btnConnect.Enabled = true;
            else btnConnect.Enabled = false;
        }

        private void tbIP_TextChanged(object sender, EventArgs e)
        {
            if (tbIP.Text != "" && tbLogin.Text != "")
                btnConnect.Enabled = true;
            else btnConnect.Enabled = false;
        }
        private void tbLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != Convert.ToChar(">")) return;
            else
                e.Handled = true;
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnSend.Enabled == true)
            {
                byte[] buff = Encoding.Unicode.GetBytes(IP.ToString() + ">>" + tbLogin.Text + ">>" + "вышел(-ла) из чата");   // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
                Stream stm = Client.GetStream();                                                    // получаем файловый поток клиентского сокета
                stm.Write(buff, 0, buff.Length);                                            // выполняем запись последовательности байт
            }

            Client.Close();         // закрытие клиентского сокета
            _continue = false;      // сообщаем, что работа с сокетами завершена

            // завершаем все потоки
            foreach (Thread t in Threads)
            {
                t.Abort();
                t.Join(500);
            }

            // закрываем клиентский сокет
            if (ServerSock != null)
                ServerSock.Close();

            // приостанавливаем "прослушивание" серверного сокета
            if (Listener != null)
                Listener.Stop();
        }

    }
}