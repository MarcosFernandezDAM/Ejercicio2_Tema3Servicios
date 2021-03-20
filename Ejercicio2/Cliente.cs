
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Ejercicio2
{
    class Cliente
    {
        private string nombre;
        public string Nombre
        {
            set
            {
                nombre = value;
            }

            get
            {
                return nombre;
            }
        }
        public Socket SocketClient { set; get; }
        public IPEndPoint ForIp { set; get; }

        public Cliente(Socket socket)
        {
            this.SocketClient = socket;
            this.ForIp = (IPEndPoint)this.SocketClient.RemoteEndPoint;
        }
    }
}