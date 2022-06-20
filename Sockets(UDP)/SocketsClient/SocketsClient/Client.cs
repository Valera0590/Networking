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
using System.Threading.Tasks;

namespace Sockets
{
    public partial class frmMain : Form
    {
        private IPAddress ipAddr;
        private Random rnd = new Random(DateTime.Now.Millisecond);
        private IPAddress IP;
        private int PortClient;
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
        private async void btnConnect_Click(object sender, EventArgs e)
        {
            ipAddr = IPAddress.Parse("127.0.0.1");
            PortClient = rnd.Next(5002, 49151);
            try
            {
                // Создаем UdpClient
                UdpClient udpClient = new UdpClient();
                // Соединяемся с удаленным хостом
                udpClient.Connect(ipAddr, 5001);

                tbLogin.ReadOnly = true;
                btnConnect.Enabled = false;
                btnSend.Enabled = true;

                byte[] data = new byte[1024];
                data = Encoding.UTF8.GetBytes(PortClient.ToString() + "!" + IP.ToString() + ">>" + tbLogin.Text + ">>" + "присоединился(-ась) к чату");
                udpClient.Send(data, data.Length);
                
                //Закрываем соединение
                udpClient.Close();   
                await ReceiveMessageAsync();   // вызов асинхронного метода
                this.Text += "     " + tbLogin.Text;
            }
            catch
            {
                MessageBox.Show("Error!");
                //tbIP.ReadOnly = false;
                tbLogin.ReadOnly = false;
                btnConnect.Enabled = true;
                btnSend.Enabled = false;
            }
        }
        private async Task ReceiveMessageAsync()
        {
            await Task.Run(() => ReceiveMessage());
        }
        // работа с серверным сокетом
        private void ReceiveMessage()
        {
            byte[] data = new byte[1024];
            string msg = "";        // полученное сообщение
           
            // Создаем UdpClient
            UdpClient udpClient = new UdpClient(PortClient);
            // входим в бесконечный цикл для работы с клиентским сокетом
            while (_continue)
            {
                // Создаем переменную IPEndPoint, чтобы передать ссылку на нее в Receive()
                IPEndPoint RemoteIPEndPoint = null;
                data = udpClient.Receive(ref RemoteIPEndPoint);
                msg = Encoding.UTF8.GetString(data);     // выполняем преобразование байтов в последовательность символов
                msg = msg.Replace("\0", "");
                if (msg != "")
                {
                    rtbChat.Invoke((MethodInvoker)delegate
                    {
                        rtbChat.Text += "\n" + msg;             // выводим полученное сообщение на форму
                    });
                }
                Thread.Sleep(200);
            }
                // Закрываем соединение
                udpClient.Close();
        }

        // отправка сообщения
        private void btnSend_Click(object sender, EventArgs e)
        {
            // Создаем UdpClient
            UdpClient udpClient = new UdpClient();
            // Соединяемся с удаленным хостом
            udpClient.Connect(ipAddr, 5001);

            byte[] data = new byte[1024];
            data = Encoding.UTF8.GetBytes(IP.ToString() + ">>" + tbLogin.Text + ">>" + tbMessage.Text);   // выполняем запись последовательности байт 
            udpClient.Send(data, data.Length);

            tbMessage.Text = "";
            // Закрываем соединение
            udpClient.Close();
        }


        private void tbLogin_TextChanged(object sender, EventArgs e)
        {
            if (/*tbIP.Text != "" && */tbLogin.Text != "")
                btnConnect.Enabled = true;
            else btnConnect.Enabled = false;
        }

        /*private void tbIP_TextChanged(object sender, EventArgs e)
        {
            if (tbIP.Text != "" && tbLogin.Text != "")
                btnConnect.Enabled = true;
            else btnConnect.Enabled = false;
        }*/
        private void tbLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != Convert.ToChar(">")) return;
            else
                e.Handled = true;
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
          5
        }

    }
}