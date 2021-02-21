using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SleepCalculator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

            string time = textBox1.Text;
            int hours = int.Parse(time.Substring(0, 2));
            int minuts = int.Parse(time.Substring(3, 2));
                if (time[2] != ':' || hours < 0 || hours > 23 || minuts > 59 || minuts < 0) throw new FormatException();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            XElement message = new XElement("Calculate", new XAttribute(sender == button1 ? "GoToBed" : "WakeUp", sender == button1 ? dateTimePicker1.Value.ToString() : dateTimePicker2.Value.ToString()), new XAttribute("Hours", textBox1.Text));
            using (Socket handler = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                handler.Connect(new IPEndPoint(IPAddress.Loopback, 8888));
                byte[] data = Encoding.Unicode.GetBytes(message.ToString());
                handler.Send(data);

                using (MemoryStream ms = new MemoryStream())
                {
                    int bytes;
                    data = new byte[4048];
                    do
                    {
                        bytes = handler.Receive(data);
                        ms.Write(data, 0, bytes);
                    } while (handler.Available > 0);

                    XElement message1 = XElement.Parse(Encoding.Unicode.GetString(ms.ToArray()));
                    dateTimePicker1.Value = DateTime.Parse(message1.Attribute("GoToBed").Value);
                    dateTimePicker2.Value = DateTime.Parse(message1.Attribute("WakeUp").Value);
                }
            }
        }

    }
}
