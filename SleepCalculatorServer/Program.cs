using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using System.Linq;

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
					{
						try
						{
							using (Socket handler = server.Accept())
							{
								XElement message;
								using (MemoryStream ms = new MemoryStream())
								{
									int bytes;
									byte[] data = new byte[4048];
									do
									{
										bytes = handler.Receive(data);
										ms.Write(data, 0, bytes);
									} while (handler.Available > 0);
									message = XElement.Parse(Encoding.Unicode.GetString(ms.ToArray()));
								}

								if (message.Name == "Calculate")
									using (SleepCalculationContext db = new SleepCalculationContext())
									{
										byte[] ipBytes = (handler.RemoteEndPoint as IPEndPoint).Address.GetAddressBytes();
										SleepCalculationRequester requester = ( // find this requester
											from r in db.Requesters
											where r.IPAddressBytes == ipBytes
											select r).FirstOrDefault();
										if (requester == null) // or create new one if he is for the first time
											requester = db.Requesters.Add(new SleepCalculationRequester() { IPAddressBytes = ipBytes });

										SleepCalculationRequest request = new SleepCalculationRequest() { Requester = requester, SleepMinutes = int.Parse(message.Attribute("SleepMinutes").Value) }, oldRequest;
										if (message.Attribute("GoToBed") == null) // requester wants to know time to go to bed having time for sleep and time to wake up
										{
											request.CalculatedValue = SleepCalculateValue.GoToBed;
											request.WakeUp = DateTime.Parse(message.Attribute("WakeUp").Value);
											oldRequest = (
												from r in db.Requests
												where r.SleepMinutes == request.SleepMinutes && r.WakeUp == request.WakeUp
												select r).FirstOrDefault();
										}
										else // requester wants to know time to wake up having time for sleep and time to go to bed
										{
											request.CalculatedValue = SleepCalculateValue.WakeUp;
											request.GoToBed = DateTime.Parse(message.Attribute("GoToBed").Value);
											oldRequest = (
												from r in db.Requests
												where r.SleepMinutes == request.SleepMinutes && r.GoToBed == request.GoToBed
												select r).FirstOrDefault();
										}

										if (oldRequest == null) // this request has never been before so calculate it and save
										{
											if (request.CalculatedValue == SleepCalculateValue.GoToBed)
												request.GoToBed = request.WakeUp.AddMinutes(-request.SleepMinutes);
											else request.WakeUp = request.GoToBed.AddMinutes(request.SleepMinutes);

											db.Requests.Add(request);
											db.SaveChanges();
										}
										else // or use already calculated data
										{
											request.GoToBed = oldRequest.GoToBed;
											request.WakeUp = oldRequest.WakeUp;

											if (request.Requester.Id != oldRequest.RequesterId) // save only if this requster has not sent this request before
											{
												db.Requests.Add(request);
												db.SaveChanges();
											}
										}

										message.Add(new XAttribute(
												request.CalculatedValue == SleepCalculateValue.GoToBed ? "GoToBed" : "WakeUp",
												(request.CalculatedValue == SleepCalculateValue.GoToBed ? request.GoToBed : request.WakeUp).ToString()));
										handler.Send(Encoding.Unicode.GetBytes(message.ToString()));
									}
								Console.WriteLine(((IPEndPoint)handler.RemoteEndPoint).Address.ToString() + " was handled.");
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
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
