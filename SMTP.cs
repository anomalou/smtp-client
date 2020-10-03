using System;
using System.IO;
using System.Net;
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
        public SMTP(string hostname, int port){
            this.hostname = hostname;
            this.port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            UTF8 = Encoding.UTF8;
            socketState = State.Idle;
        }

        public void SetFromAddress(string address){
            fromAddress = address;
        }

        public void SetToAddress(string address){
            toAddress = address;
        }

        public void Login(string username, string password){
            this.username = username;
            this.password = password;
        }

        public void CreateEMail(string subject, string text){
            socketState = State.Idle;
            package = "From: " + fromAddress + "\r\nTo: " + toAddress + "\r\nSubject: " + subject + "\r\n\n" + text;
            Console.WriteLine("---Email created! From: " + fromAddress + " To: " + toAddress + "---\n---Connection with server estabilished!---\n");
        }

        public void Send(){
            if(hostname != "" && port != 0 && username != "" && password != "" && fromAddress != "" && toAddress != ""){
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
                Transfer();
            }else{
                Console.WriteLine("Error! Not all fields was not filled!\n");
            }
        }

        Byte[] GetBytes(string message){
            return UTF8.GetBytes(message);
        }

        string GetString(Byte[] bytes){
            return UTF8.GetString(bytes);
        }

        void Transfer(){
            if(socketState == State.Idle && successCode == 220){
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
            }else if(socketState == State.MailFrom && successCode == 235){
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
                return;   
            }

            Byte[] bytePackage = new Byte[2048];
            socket.Receive(bytePackage);
            string[] output = GetString(bytePackage).Split(' ', 2);
            successCode = int.Parse(output[0]);
            Console.WriteLine($"{output[0]} {output[1]}");


            if(socketState == State.Shutdown && successCode == 250){
                Console.WriteLine("Email successfuly sent!\n");
                socket.Close();
            }else{
                Transfer();
            }
        }
    }
}