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
using System.Messaging;
using System.Threading.Tasks;

namespace MSMQ
{
    public partial class frmMain : Form
    {
        private MessageQueue q = null;          // очередь сообщений
        private Thread t = null;                // поток, отвечающий за работу с очередью сообщений
        private bool _continue = true;          // флаг, указывающий продолжается ли работа с мэйлслотом
        private List<string> LoginClients = new List<string>();
        private MessageQueue qCl = null;          // очередь сообщений


        // конструктор формы
        public frmMain()
        {
            InitializeComponent();
            string path = Dns.GetHostName() + "\\private$\\ServerQueue";    // путь к очереди сообщений, Dns.GetHostName() - метод, возвращающий имя текущей машины

            // если очередь сообщений с указанным путем существует, то открываем ее, иначе создаем новую
            if (MessageQueue.Exists(path))
                q = new MessageQueue(path);
            else
                q = MessageQueue.Create(path);

            // задаем форматтер сообщений в очереди
            q.Formatter = new XmlMessageFormatter(new Type[] { typeof(String) });

            // вывод пути к очереди сообщений в заголовок формы, чтобы можно было его использовать для ввода имени в форме клиента, запущенного на другом вычислительном узле
            this.Text += "     " + q.Path;
            LoginClients.Clear();

            // создание потока, отвечающего за работу с очередью сообщений
            //Thread t = new Thread(ReceiveMessage);
            //t.Start();
            RecMes();
        }
        private async void RecMes()
        {
            await ReceiveMessageAsync();   // вызов асинхронного метода
        }

        private async Task ReceiveMessageAsync()
        {
            await Task.Run(() => ReceiveMessage());
        }

        // получение сообщения
        private void ReceiveMessage()
        {
            if (q == null)
                return;
            string path = Dns.GetHostName() + "\\private$\\ClientQueue";    // путь к очереди сообщений, Dns.GetHostName() - метод, возвращающий имя текущей машины
            System.Messaging.Message msg = null;
            if (MessageQueue.Exists(path))
            {
                // если очередь, путь к которой указан в поле tbPath существует, то открываем ее
                qCl = new MessageQueue(path);
            }
            else    MessageBox.Show("Указан неверный путь к очереди, либо очередь не существует");
            // входим в бесконечный цикл работы с очередью сообщений
            while (_continue)
            {
                if (q.Peek() != null)   // если в очереди есть сообщение, выполняем его чтение, интервал до следующей попытки чтения равен 10 секундам
                    msg = q.Receive(TimeSpan.FromSeconds(10.0));
                MsgProcessing(msg);
                rtbMessages.Invoke((MethodInvoker)delegate
                {
                    if (msg != null)
                        rtbMessages.Text += "\n >> " + msg.Label + " : " + msg.Body;     // выводим полученное сообщение на форму
                });
                Thread.Sleep(150);              // синхронизация удаления клиента и отправки сообщений всем клиентам
                for (int i = 0; i < LoginClients.Count(); i++)     // отправка сообщений всем присоединившимся клиентам
                {
                    // выполняем отправку сообщения в очередь
                    qCl.Send(msg.Body, msg.Label);
                }
                Thread.Sleep(500);          // приостанавливаем работу потока перед тем, как приcтупить к обслуживанию очередного клиента
            }
        }

        private void MsgProcessing(System.Messaging.Message msg)
        {
            string message = msg.Body.ToString();

            if (!LoginClients.Contains(msg.Label.ToString()))               // при первом подключении клиента
            {
                LoginClients.Add(msg.Label.ToString());
            }
            if (message == "вышел(-ла) из чата")        // при выходе клиента из чата
            {
                LoginClients.Remove(msg.Label.ToString());
            }

        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _continue = false;      // сообщаем, что работа с очередью сообщений завершена

            /*if (t != null)
            {
                t.Abort();          // завершаем поток
            }*/

            if (q != null)
            {
                //MessageQueue.Delete(q.Path);      // в случае необходимости удаляем очередь сообщений
            }
        }
    }
}