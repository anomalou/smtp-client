using System;
using System.Text;
using System.Net.Sockets;

namespace smtp_client_csharp{
    class SMTP{
        enum State{
            Idle,
            Auth,
            AuthLogin,
            AuthPassword,
            MailFrom,
            MailTo,
            Mail,
            Sending,
            Shutdown
        }

        char[] separators;
        string hostname;
        int port;

        string username;
        string password;

        string fromAddress;
        string toAddress;

        string package;

        Encoding UTF8;

        Socket socket;

        State socketState;
        int successCode;

        bool _authorithation;
        bool _connected;
        public bool authorithation{get{return _authorithation;}}
        public bool connected{get{return _connected;}}

        public SMTP(string hostname, int port){
            this.hostname = hostname;
            this.port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            UTF8 = Encoding.UTF8;
            socketState = State.Idle;
            _authorithation = false;
            _connected = false;
        }

        public void Connect(){
            if(hostname != "" && port != 0){
                try{
                    socket.Connect(hostname, port);
                }catch(Exception ex){
                    Console.WriteLine(ex.Message);
                }
                if(!socket.Connected){
                    Console.WriteLine("Error! Cannot connect to server!");
                    return;
                }
                Byte[] bytePackage = new Byte[2048];
                socket.Receive(bytePackage);
                successCode = int.Parse(GetString(bytePackage).Split(' ', 2)[0]);
                Console.WriteLine(GetString(bytePackage));
                if(successCode == 220){
                    _connected = true;
                }else
                    Console.WriteLine("Error!");
            }else{
                Console.WriteLine("Error! No hostname or port!\n");
            }
        }

        public void Login(string username, string password){
            if(socketState == State.Idle && connected){
                socket.Send(GetBytes("HELO mail.oreluniver.ru\r\n"));
                socketState = State.Auth;
            }else if(socketState == State.Auth && successCode == 250){
                socket.Send(GetBytes("AUTH LOGIN\r\n"));
                socketState = State.AuthLogin;
            }else if(socketState == State.AuthLogin && successCode == 334){
                socket.Send(GetBytes(System.Convert.ToBase64String(GetBytes(username)) + "\r\n"));
                socketState = State.AuthPassword;
            }else if(socketState == State.AuthPassword && successCode == 334){
                socket.Send(GetBytes(System.Convert.ToBase64String(GetBytes(password)) + "\r\n"));
                socketState = State.MailFrom;
            }else{
                socketState = State.Idle;
                Disconnect();
                Console.WriteLine("Error!");
                return;
            }

            Byte[] bytePackage = new Byte[2048];
            socket.Receive(bytePackage);
            string[] output = GetString(bytePackage).Split(' ', 2);
            successCode = int.Parse(output[0]);
            Console.WriteLine($"{output[0]} {output[1]}");
        
            if(socketState == State.MailFrom && successCode == 235){
                Console.WriteLine("Authorithated!\n");
                this.username = username;
                this.password = password;
                _authorithation = true;
            }else{
                Login(username, password);
            }
        }

        

        public void CreateEMail(string fromAddress, string toAddress, string subject, string text){
            this.fromAddress = fromAddress;
            this.toAddress = toAddress;
            package = "From: " + fromAddress + "\r\nTo: " + toAddress + "\r\nSubject: " + subject + "\r\n\n" + text;
            Console.WriteLine("---Email created! From: " + fromAddress + " To: " + toAddress + "---");
        }

        Byte[] GetBytes(string message){
            return UTF8.GetBytes(message);
        }

        string GetString(Byte[] bytes){
            return UTF8.GetString(bytes);
        }

        public void Send(){
            if(socketState == State.MailFrom && authorithation){
                socket.Send(GetBytes("MAIL FROM:<" + fromAddress + ">\r\n"));
                socketState = State.MailTo;
            }else if(socketState == State.MailTo && successCode == 250){
                socket.Send(GetBytes("RCPT TO:<" + toAddress + ">\r\n"));
                socketState = State.Mail;
            }else if(socketState == State.Mail && successCode == 250){
                socket.Send(GetBytes("DATA\r\n"));
                socketState = State.Sending;
            }else if(socketState == State.Sending && successCode == 354){
                socket.Send(GetBytes(package + "\r\n.\r\n"));
                socketState = State.Shutdown;
            }else{
                socketState = State.MailFrom;
                Console.WriteLine("Error!");
                Disconnect();
                return;   
            }

            Byte[] bytePackage = new Byte[2048];
            socket.Receive(bytePackage);
            string[] output = GetString(bytePackage).Split(' ', 2);
            successCode = int.Parse(output[0]);
            Console.WriteLine(GetString(bytePackage));


            if(socketState == State.Shutdown && successCode == 250){
                Console.WriteLine("Email successfuly sent!\n");
                socketState = State.MailFrom;
            }else{
                Send();
            }
        }

        public void Disconnect(){
            Byte[] bytes = new Byte[256];
            if(socket.Connected){
                socket.Send(GetBytes("QUIT\r\n"));
                socket.Receive(bytes);
                Console.WriteLine(GetString(bytes));
                socket.Close();
                _connected = false;
                Console.WriteLine("Socket disconnected!");
            }else if(!socket.Connected && !_connected){
                Console.WriteLine("Error! Socket already disconnected!");
            }else if(!socket.Connected && _connected){
                Console.WriteLine("Error! Your socket has been disconnected by yourself!");
            }
        }
    }
}