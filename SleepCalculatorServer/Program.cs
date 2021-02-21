using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
                    Console.WriteLine("Server started.");
					while (true)
						try
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

										string[] s = message.Attribute("Sleep").Value.Split(':');
										TimeSpan ts = new TimeSpan(int.Parse(s[0]), int.Parse(s[1]), 0);
										message.Add(new XAttribute(a.Name == "GoToBed" ? "WakeUp" : "GoToBed", (a.Name == "GoToBed" ? DateTime.Parse(a.Value).Add(ts) : DateTime.Parse(a.Value).Subtract(ts)).ToString()));

										data = Encoding.Unicode.GetBytes(message.ToString());
										handler.Send(data);
									}
								}
								Console.WriteLine(handler.LocalEndPoint.ToString() + " was handled.");
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
				}
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex.Message);
                Console.WriteLine("Server stopped.");
			}
		}
    }
}
