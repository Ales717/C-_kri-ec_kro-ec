using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Security.Cryptography;

namespace Client
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
                return Encoding.UTF8.GetString(buf, 0, length);
            }
            catch (Exception e)
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
            catch (Exception e)
            {
                Console.WriteLine("napačen vnos" + e.Message);
            }
        }
        public static string Decrypt(string msg)
        {
            try
            {
                byte[] decrypt = Convert.FromBase64String(msg);
                MD5CryptoServiceProvider myMD5 = new MD5CryptoServiceProvider();
                byte[] securityKeyArray = myMD5.ComputeHash(UTF8Encoding.UTF8.GetBytes(std_key));
                myMD5.Clear();

                var tripleDES = new TripleDESCryptoServiceProvider();
                tripleDES.Key = securityKeyArray;
                tripleDES.Mode = CipherMode.ECB;
                var crytpoTransform = tripleDES.CreateDecryptor();
                byte[] result = crytpoTransform.TransformFinalBlock(decrypt, 0, decrypt.Length);
                tripleDES.Clear();

                return UTF8Encoding.UTF8.GetString(result);
            }
            catch (Exception e)
            {
                Console.WriteLine("napačen vnos" + e.Message);
                return null;
            }

        }
        static void Main(string[] args)
        {
            Console.WriteLine("Odjemalec");
            while (run)
            {
                Console.WriteLine("\n Vnesi ukaz: ");
                string input = Console.ReadLine();

                if (input == "q") break;
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(ip, port);

                    NetworkStream ns = client.GetStream();
                    Send(ns, input);
                    string response = Receive(ns);
                    if (response[0] == 'G')
                    {
                        string body = response.Substring(std_head, response.Length - 1);
;
                        Console.WriteLine("strežnik vračadd: " + Decrypt(body));
                    }
                    else
                    {
                        Console.WriteLine("strežnik vrača: " + response);
                    }

                    

                }
                catch(Exception e)
                {
                    Console.WriteLine("Napaka: " + e.Message);
                }
                
            }
            Console.WriteLine("Pritisni poljubno tipko za zaključek programa...");
            Console.ReadKey();
        }
    }
}
