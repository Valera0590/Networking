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
using System.Messaging;
using System.Threading.Tasks;
using System.Threading;

namespace MSMQ
{
    public partial class frmMain : Form
    {
        private MessageQueue q = null;      // очередь сообщений, в которую будет производиться запись сообщений
        private bool _continue = true;          // флаг, указывающий продолжается ли работа с мэйлслотом
        private MessageQueue qCl = null;      // очередь сообщений, в которую будет производиться запись сообщений


        // конструктор формы
        public frmMain()
        {
            InitializeComponent();
            string path = Dns.GetHostName() + "\\private$\\ClientQueue";    // путь к очереди сообщений, Dns.GetHostName() - метод, возвращающий имя текущей машины

            // если очередь сообщений с указанным путем существует, то открываем ее, иначе создаем новую
            if (MessageQueue.Exists(path))
                qCl = new MessageQueue(path);
            else
                qCl = MessageQueue.Create(path);

            // задаем форматтер сообщений в очереди
            qCl.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            if (MessageQueue.Exists(tbPath.Text))
            {
                // если очередь, путь к которой указан в поле tbPath существует, то открываем ее
                q = new MessageQueue(tbPath.Text);
                btnSend.Enabled = true;
                btnConnect.Enabled = false;
                tbPath.Enabled = false;
                tbLogin.Enabled = false;
                string message = "присоединился(-ась) к чату";
                if (tbLogin.Text == "")
                {
                    // выполняем отправку сообщения в очередь
                    q.Send(message, Dns.GetHostName());
                    this.Text += "     " + Dns.GetHostName();
                }
                else
                {
                    q.Send(message, tbLogin.Text);
                    this.Text += "     " + tbLogin.Text;
                }
                await ReceiveMessageAsync();   // вызов асинхронного метода
            }
            else
                MessageBox.Show("Указан неверный путь к очереди, либо очередь не существует");
        }

        private async Task ReceiveMessageAsync()
        {
            await Task.Run(() => ReceiveMessage());
        }

        // получение сообщения
        private void ReceiveMessage()
        {
            if (qCl == null)
                return;
            System.Messaging.Message msg = null;
            
            // входим в бесконечный цикл работы с очередью сообщений
            while (_continue)
            {
                if (qCl.Peek() != null)   // если в очереди есть сообщение, выполняем его чтение, интервал до следующей попытки чтения равен 10 секундам
                    msg = qCl.Receive(TimeSpan.FromSeconds(10.0));

                rtbMessages.Invoke((MethodInvoker)delegate
                {
                    if (msg != null)
                        rtbMessages.Text += "\n >> " + msg.Label + " : " + msg.Body;     // выводим полученное сообщение на форму
                });

                Thread.Sleep(500);          // приостанавливаем работу потока перед тем, как приcтупить к обслуживанию очередного клиента
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (tbLogin.Text == "")
            {
                // выполняем отправку сообщения в очередь
                q.Send(tbMessage.Text, Dns.GetHostName());
            }
            else q.Send(tbMessage.Text, tbLogin.Text);
            tbMessage.Text = "";
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (btnSend.Enabled == true)
            {
                string message = "вышел(-ла) из чата";
                if (tbLogin.Text == "")
                {
                    // выполняем отправку сообщения в очередь
                    q.Send(message, Dns.GetHostName());
                    this.Text += "     " + Dns.GetHostName();
                }
                else    q.Send(message, tbLogin.Text);

            }

            _continue = false;      // сообщаем, что работа с очередью завершена
        }

        private void tbLogin_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != Convert.ToChar(">")) return;
            else
                e.Handled = true;
        }
    }
}