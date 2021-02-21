using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SleepCalculatorServer
{
    class Program
    {
        static void Main(string[] args)
        {
			try
			{
				using (Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
				{
					server.Bind(new IPEndPoint(IPAddress.Any, 8888));
					server.Listen(10);
					while (true)
					{
						using (Socket handler = server.Accept())
						{
							using (MemoryStream ms = new MemoryStream())
                            {
								int bytes;
								byte[] data = new byte[4048];
								do
								{
									bytes = handler.Receive(data);
									ms.Write(data, 0, bytes);
								} while (handler.Available > 0);

								XElement message = XElement.Parse(Encoding.Unicode.GetString(ms.ToArray()));
								if (message.Name == "Calculate")
								{
									XAttribute a = message.Attribute("GoToBed");
									if (a == null)
										a = message.Attribute("WakeUp");
									int hours = int.Parse(message.Attribute("Hours").Value);
									DateTime time = DateTime.Parse(a.Value).AddHours(a.Name == "GoToBed" ? hours : -hours);
									message.Add(new XAttribute(a.Name == "GoToBed" ? "WakeUp" : "GoToBed", time.ToString()));
									data = Encoding.Unicode.GetBytes(message.ToString());
									handler.Send(data);
								}
                            }
						}
					}
				}
			}
			catch (Exception ex)
			{

			}
		}
    }
}
