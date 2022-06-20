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
using System.Collections;
using System.IO;

namespace Sockets
{
    public partial class frmMain : Form
    {
        private Socket ClientSock;                      // клиентский сокет
        private TcpListener Listener;                   // сокет сервера
        private List<Thread> Threads = new List<Thread>();      // список потоков приложения (кроме родительского)
        private bool _continue = true;                          // флаг, указывающий продолжается ли работа с сокетами
        private List<IPAddress> IPClients = new List<IPAddress>();
        private List<string> LoginClients = new List<string>();
        private List<int> PortClients = new List<int>();

        // конструктор формы
        public frmMain()
        {
            InitializeComponent();

            IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());    // информация об IP-адресах и имени машины, на которой запущено приложение
            IPAddress IP = hostEntry.AddressList[0];                        // IP-адрес, который будет указан при создании сокета
            int Port = 1010;                                                // порт, который будет указан при создании сокета

            // определяем IP-адрес машины в формате IPv4
            foreach (IPAddress address in hostEntry.AddressList)
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    IP = address;
                    break;
                }

            // вывод IP-адреса машины и номера порта в заголовок формы, чтобы можно было его использовать для ввода имени в форме клиента, запущенного на другом вычислительном узле
            this.Text += "     " + IP.ToString() + "  :  " + Port.ToString();

            LoginClients.Clear();
            PortClients.Clear();
            IPClients.Clear();

            // создаем серверный сокет (Listener для приема заявок от клиентских сокетов)
            Listener = new TcpListener(IP, Port);
            Listener.Start();

            // создаем и запускаем поток, выполняющий обслуживание серверного сокета
            Threads.Clear();
            Threads.Add(new Thread(ReceiveMessage));
            Threads[Threads.Count-1].Start();
        }

        // работа с клиентскими сокетами
        private void ReceiveMessage()
        {
            // входим в бесконечный цикл для работы с клиентскими сокетом
            while (_continue)
            {
                ClientSock = Listener.AcceptSocket();           // получаем ссылку на очередной клиентский сокет
                Threads.Add(new Thread(ReadMessages));          // создаем и запускаем поток, обслуживающий конкретный клиентский сокет
                Threads[Threads.Count - 1].Start(ClientSock);
            }
        }

        // получение сообщений от конкретного клиента
        private void ReadMessages(object ClientSock)
        {
            string msg = "";        // полученное сообщение
            string[] ArrMsgs = new string[3];
            
            // входим в бесконечный цикл для работы с клиентским сокетом
            while (_continue)
            {
                byte[] buff = new byte[1024];                           // буфер прочитанных из сокета байтов
                ((Socket)ClientSock).Receive(buff);                     // получаем последовательность байтов из сокета в буфер buff
                msg = System.Text.Encoding.Unicode.GetString(buff);     // выполняем преобразование байтов в последовательность символов
                msg = msg.Replace("\0", "");
                if (msg != "")
                {
                    ArrMsgs = SplitMsg(msg);
                    rtbMessages.Invoke((MethodInvoker)delegate
                    {
                            rtbMessages.Text += "\n >>  " + ArrMsgs[1] + "  >>  " + ArrMsgs[2];             // выводим полученное сообщение на форму
                    });
                    for (int i = 0; i < IPClients.Count(); i++)     // отправка сообщений всем присоединившимся клиентам
                    {
                        TcpClient Server = new TcpClient();     // серверный сокет
                        Server.Connect(IPClients[i], PortClients[i]);                       // подключение к клиентскому сокету
                        byte[] buff_cl = Encoding.Unicode.GetBytes(" >>  " + ArrMsgs[1] + "  >>  " + ArrMsgs[2]);   // выполняем преобразование сообщения (вместе с идентификатором машины) в последовательность байт
                        Stream stm = Server.GetStream();                                                    // получаем файловый поток клиентского сокета
                        stm.Write(buff_cl, 0, buff_cl.Length);                                                    // выполняем запись последовательности байт
                        Server.Close();         // закрытие клиентского сокета
                    }
                }
                Thread.Sleep(500);
            }
        }
        private string[] SplitMsg(string message)
        {
            string[] ArrayMessages = new string[3];
            for (int i = 0; i < ArrayMessages.Count(); i++)
            {
                ArrayMessages[i] = "";
            }
            //message = message.Replace(" ", "");
            //string temp = "";
            int j = 0;
            while(message != "")
            {
                if (message[0] == '>' && j < 2)
                {
                    message = message.Remove(0, 2);
                    j++;
                }
                else
                {
                    ArrayMessages[j] += message[0];
                    message = message.Remove(0, 1);
                }
            }
            
            if (!LoginClients.Contains(ArrayMessages[1]))               // при первом подключении клиента
            {
                string[] temp = ArrayMessages[0].Split('!');
                ArrayMessages[0] = temp[1];
                PortClients.Add(Int32.Parse(temp[0]));
                LoginClients.Add(ArrayMessages[1]);
                IPClients.Add(IPAddress.Parse(ArrayMessages[0]));
            }
            if (ArrayMessages[2] == "вышел(-ла) из чата")           // при выходе клиента из чата
            {
                PortClients.RemoveAt(LoginClients.IndexOf(ArrayMessages[1]));
                LoginClients.Remove(ArrayMessages[1]);
                IPClients.Remove(IPAddress.Parse(ArrayMessages[0]));
            }
            return ArrayMessages;
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;      // сообщаем, что работа с сокетами завершена
            
            // завершаем все потоки
            foreach (Thread t in Threads)
            {
                t.Abort();
                t.Join(500);
            }

            // закрываем клиентский сокет
            if (ClientSock != null)
                ClientSock.Close();

            // приостанавливаем "прослушивание" серверного сокета
            if (Listener != null)
                Listener.Stop();
        }
    }
}