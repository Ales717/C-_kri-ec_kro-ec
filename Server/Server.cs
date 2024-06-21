using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    class Program
    {
        static bool run = true;
        const int port = 1234;
        const string ip = "127.0.0.1";
        const int msg = 1024;
        const int std_head = 1;
        const string std_key = "kluc";

    
        static string Receive(NetworkStream ns)
        {
            try
            {
                byte[] buf = new byte[msg];
                int length = ns.Read(buf, 0, buf.Length);
                string rec = Encoding.UTF8.GetString(buf, 0, length);
                return rec;
            }
            catch(Exception e)
            {
                Console.WriteLine("napačen vnos" + e.Message);
                return null;
            }
            
        }
        static void Send(NetworkStream ns, string msg)
        {
            try
            {
                byte[] send = Encoding.UTF8.GetBytes(msg.ToCharArray(), 0, msg.Length);
                ns.Write(send, 0, send.Length);
            }
            catch(Exception e)
            {
                Console.WriteLine("napačen vnos" + e.Message);
            }
        }
        public static string Encripty(string msg)
        {
            try
            {
                byte[] encrypted = UTF8Encoding.UTF8.GetBytes(msg);
                MD5CryptoServiceProvider myMD5 = new MD5CryptoServiceProvider();
                byte[] securityKeyArray = myMD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(std_key));
                myMD5.Clear();

                var tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = securityKeyArray;
                tripleDES.Mode = CipherMode.ECB;
                var crytpoTransform = tripleDES.CreateEncryptor();
                byte[] result = crytpoTransform.TransformFinalBlock(encrypted, 0, encrypted.Length);
                tripleDES.Clear();

                return Convert.ToBase64String(result, 0, result.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("napačen vnos" + e.Message);
                return null;
            }

        }
        static void Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(ip), port);
            listener.Start();
            Console.WriteLine("Strežnik posluša na naslovu:" + ip + ":" + port.ToString());

            while (run)
            {
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream ns = client.GetStream())
                {
                    Console.WriteLine("odjemalec se je povezal (" + client.Client.RemoteEndPoint.ToString() + ")");
                    string msg = Receive(ns);
                    Console.WriteLine("Dobil sm sporočlo" + msg);
                    string response = "";

                    string head = msg[0].ToString();
                    string body = "";
                    if(msg.Length > 1)
                    {
                        body = msg.Substring(std_head, msg.Length - 1);
                    }
                    switch (head)
                    {
                        case "A":
                            response = "pozdravljeni " + client.Client.RemoteEndPoint.ToString();
                            break;
                        case "B":
                            response = "Trenutni čas:  " + DateTime.Now;
                            break;
                        case "C":
                            response = "Trenutni delovni direktorij:  " + Directory.GetCurrentDirectory();
                            break;
                        case "D":
                            response = body;
                            break;
                        case "E":
                            response = "Ime računalnika: " +  Environment.MachineName + ", verzija OSa: " + Environment.OSVersion;
                            break;
                        case "G":
                            response = "G" + Encripty(body);
                            break;
                        default:
                            response = "Nepravilen protokol";
                            break;
                    }

                    Send(ns, response);
                    Console.WriteLine("odgovoril sem: " + response);
                }
                Console.WriteLine("Odjemalec se je odklopil.\n");
            }
            listener.Stop();
            Console.WriteLine("strežnik se je ustaavil");
            Console.ReadKey();
        }

    }
}