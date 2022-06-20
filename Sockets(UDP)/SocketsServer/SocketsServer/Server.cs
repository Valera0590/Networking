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
using System.Threading.Tasks;

namespace Sockets
{
    public partial class frmMain : Form
    {
        private bool _continue = true;                          // флаг, указывающий продолжается ли работа с сокетами
        private IPAddress IP;                           // IP-адрес клиента
        private List<IPAddress> IPClients = new List<IPAddress>();
        private List<string> LoginClients = new List<string>();
        private List<int> PortClients = new List<int>();
        private IPAddress ipAddr = IPAddress.Parse("127.0.0.1");

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

            LoginClients.Clear();
            PortClients.Clear();
            IPClients.Clear();

            AsnStart();
        }
        
        private async void AsnStart()
        {
            await ReceiveMessageAsync();        //вызов асинхронного метода обработки сообщений
        }

        private async Task ReceiveMessageAsync()
        {
            await Task.Run( () => ReceiveMessage());
        }

        // получение сообщений от клиента
        private void ReceiveMessage()
        {
            byte[] data = new byte[1024];
            string msg = "";        // полученное сообщение
            string[] ArrMsgs = new string[3];

            // входим в бесконечный цикл для работы с клиентским сокетом
            while (_continue)
            {
                // Создаем UdpClient
                UdpClient udpClient = new UdpClient(5001);
                // Создаем переменную IPEndPoint, чтобы передать ссылку на нее в Receive()
                IPEndPoint RemoteIPEndPoint = null;
                data = udpClient.Receive(ref RemoteIPEndPoint);
                msg = Encoding.UTF8.GetString(data);     // выполняем преобразование байтов в последовательность символов
                msg = msg.Replace("\0", "");
                if (msg != "")
                {
                    ArrMsgs = SplitMsg(msg);
                    rtbMessages.Invoke((MethodInvoker)delegate
                    {
                        rtbMessages.Text += "\n >>  " + ArrMsgs[1] + "  >>  " + ArrMsgs[2];             // выводим полученное сообщение на форму
                    });
                    Thread.Sleep(100);
                    for (int i = 0; i < LoginClients.Count(); i++)     // отправка сообщений всем присоединившимся клиентам
                    {
                        UdpClient udpServer = new UdpClient();

                        // Соединяемся с удаленным хостом
                        udpServer.Connect(ipAddr, PortClients[i]);

                        byte[] data_s = new byte[1024];
                        data_s = Encoding.UTF8.GetBytes(" >>  " + ArrMsgs[1] + "  >>  " + ArrMsgs[2]);   // выполняем запись последовательности байт 
                        udpServer.Send(data_s, data_s.Length);

                        // Закрываем соединение
                        udpServer.Close();
                    }
                }
                // Закрываем соединение
                udpClient.Close();
                Thread.Sleep(100);
            }

        }
        private string[] SplitMsg(string message)
        {
            string[] ArrayMessages = new string[3];
            for (int i = 0; i < ArrayMessages.Count(); i++)
            {
                ArrayMessages[i] = "";
            }
            
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
            if (ArrayMessages[2] == "вышел(-ла) из чата")
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
        }
    }
}