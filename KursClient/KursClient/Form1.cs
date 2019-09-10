using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace KursClient
{
    public partial class Form1 : Form
    {
        private const int listenPort = 11000;
        private const int SIZE = 2048;
        Socket listener;

        public Form1()
        {
            InitializeComponent();

            textBox1.MaxLength = 12;
            textBox2.MaxLength = 6;

            if (!checkBox1.Checked)
            {
                textBox2.PasswordChar = '*';
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String textClient;
            String textServer;

            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text == "")
            {
                textClient = "<Create>" + "<Username>" + textBox1.Text + "<Password>" + textBox2.Text + "<End>";
            }
            else if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "")
            {
                textClient = "<CreateG>" + "<Username>" + textBox1.Text + "<Password>" + textBox2.Text + "<Groupname>" + textBox3.Text + "<End>";
            }
            else if (textBox1.Text == "" && textBox2.Text == "" && textBox3.Text != "")
            {
                textClient = "<Group>" + "<Groupname>" + textBox3.Text + "<End>";
            }
            else
            {
                ErrMessage("Заполните поля!!!");
                return;
            }

            textServer = Send(textClient);
            richTextBox1.Clear();
            richTextBox1.AppendText(textServer);
        }

        private void ErrMessage(string mes)
        {
            richTextBox1.Clear();
            richTextBox1.AppendText(mes);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String textClient = "<OutputU>" + "<End>";
            string textServer = Send(textClient);
            richTextBox1.Clear();
            richTextBox1.AppendText(textServer);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            String textClient;
            String textServer;

            if (textBox1.Text != "" && textBox3.Text == "")
            {
                textClient = "<Delete>" + "<Username>" + textBox1.Text + "<End>";
            }
            else if (textBox3.Text != "" && textBox1.Text == "")
            {
                textClient = "<DeleteG>" + "<Groupname>" + textBox3.Text + "<End>";
            }
            else
            {
                ErrMessage("Для удаления необходимо заполнить 1 из полей: пользователь или имя группы!!!");
                return;
            }

            textServer = Send(textClient);
            richTextBox1.Clear();
            richTextBox1.AppendText(textServer);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox2.PasswordChar = '\0';
            }
            else
            {
                textBox2.PasswordChar = '*';
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            String textClient;
            String textServer;
            textClient = "<OutputG>" + "<End>";
            textServer = Send(textClient);
            richTextBox1.Clear();
            richTextBox1.AppendText(textServer);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            String textClient;
            String textServer;

            if (textBox1.Text != "" && textBox3.Text != "")
            {
                textClient = "<SetGroup>" + "<Username>" + textBox1.Text + "<Groupname>" + textBox3.Text + "<End>";
            }
            else
            {
                ErrMessage("Заполните поля!!!");
                return;
            }
            textServer = Send(textClient);
            richTextBox1.Clear();
            richTextBox1.AppendText(textServer);
        }

        private string Send(string mess)
        {
            try
            {
                IPHostEntry ipHost = Dns.Resolve("localhost");
                IPAddress ipAdr = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAdr, listenPort);
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                String textClient = mess;
                byte[] byteSend = Encoding.UTF8.GetBytes(textClient);
                listener.SendTo(byteSend, ipEndPoint);
                
                byte[] byteRec = new byte[SIZE];
                int len = listener.Receive(byteRec);
                string textServer = Encoding.UTF8.GetString(byteRec, 0, len);
                return "От сервера получено: " + "\n" + textServer;
            }
            catch (Exception mesEx)
            {
                return "Server error!";
            }
            listener.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
        }
    }
}
