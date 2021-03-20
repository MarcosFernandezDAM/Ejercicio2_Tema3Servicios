using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ejercicio2
{
    class Program
    {
        static object l = new Object();

        static List<Cliente> clientes;

        static void Main(string[] args)
        {
            int puerto = 31416;
            Thread thread;
            clientes = new List<Cliente>();
            IPHostEntry info = Dns.GetHostEntry("localhost");

            IPEndPoint ie = new IPEndPoint(IPAddress.Any, puerto);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(ie);
                Console.WriteLine("El puerto es válido");
                socket.Listen(10);
                while (true)
                {
                    Socket cliente = socket.Accept();
                    Cliente c = new Cliente(cliente);
                    lock (l)
                    {
                        clientes.Add(c);
                    }
                    thread = new Thread(ClienteThread);
                    thread.Start(c);
                }

            }
            catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine("Error, el puerto ya está en uso");
            }
            socket.Close();
            Console.ReadLine();
        }

        static void ClienteThread(object socket)
        {
            bool salir = false;
            string mensaje;
            Cliente clienteObject = (Cliente)socket;

            Socket cliente = clienteObject.SocketClient;
            IPEndPoint ieCliente = (IPEndPoint)cliente.RemoteEndPoint;
            Console.WriteLine("Conectado al puerto {0}", ieCliente.Port);

            using (NetworkStream ns = new NetworkStream(cliente))
            using (StreamReader sr = new StreamReader(ns))
            using (StreamWriter sw = new StreamWriter(ns))
            {

                sw.WriteLine("Bienvenido, cómo te llamas?");
                sw.Flush();
                string nombre = sr.ReadLine();
                clienteObject.Nombre = nombre;

                lock (l)
                {
                    for (int i = 0; i < clientes.Count; i++)
                    {
                        if (cliente != clientes[i].SocketClient)
                        {
                            using (NetworkStream nsInside = new NetworkStream(clientes[i].SocketClient))
                            using (StreamWriter swInside = new StreamWriter(nsInside))
                            {
                                swInside.WriteLine("{0} se ha conectado", nombre);
                                swInside.Flush();
                            }
                        }

                    }
                }

                while (!salir)
                {
                    try
                    {

                        mensaje = sr.ReadLine();
                        string mensajeMayus = mensaje.ToUpper();
                        if (mensajeMayus != null)
                        {
                            switch (mensajeMayus)
                            {
                                case "#SALIR":
                                    lock (l)
                                    {
                                        clientes.Remove(clienteObject);
                                        for (int i = 0; i < clientes.Count; i++)
                                        {
                                            using (NetworkStream nsInside = new NetworkStream(clientes[i].SocketClient))
                                            using (StreamWriter swInside = new StreamWriter(nsInside))
                                            {
                                                swInside.WriteLine("{0} se ha desconectado ", clienteObject.Nombre);
                                                swInside.Flush();
                                            }
                                        }
                                    }
                                    salir = true;
                                    break;
                                case "#LISTA":
                                    sw.WriteLine("Personas conectadas al servidor:");
                                    sw.Flush();
                                    lock (l)
                                    {
                                        for (int i = 0; i < clientes.Count; i++)
                                        {
                                            sw.WriteLine(clientes[i].Nombre);
                                            sw.Flush();
                                        }
                                    }
                                    break;
                                default:
                                    lock (l)
                                    {
                                        for (int i = 0; i < clientes.Count; i++)
                                        {
                                            if (cliente != clientes[i].SocketClient)
                                            {
                                                using (NetworkStream nsInside = new NetworkStream(clientes[i].SocketClient))
                                                using (StreamWriter swInside = new StreamWriter(nsInside))
                                                {
                                                    swInside.WriteLine("{0}@{1}:{2} ", clienteObject.Nombre, clienteObject.ForIp.Address, mensaje);
                                                    swInside.Flush();
                                                }
                                            }

                                        }
                                    }
                                    break;
                            }
                        }
                        else
                        {

                            for (int i = 0; i < clientes.Count; i++)
                            {
                                using (NetworkStream nsInside = new NetworkStream(clientes[i].SocketClient))
                                using (StreamWriter swInside = new StreamWriter(nsInside))
                                {
                                    swInside.WriteLine("{0} se ha desconectado ", clienteObject.Nombre);
                                    swInside.Flush();
                                }
                            }

                            salir = true;
                            cliente.Close();

                        }

                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e.Message);
                    }

                }

                lock (l) 
                {
                    salir = true;
                    cliente.Close();
                    clientes.Remove(clienteObject);
                }
            }
        }
    }
}
