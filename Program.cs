using System;

namespace smtp_client_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            bool stop = false;
            bool questionStop = false;
            string from, to, username, password, subject, text;
            SMTP smtp = new SMTP("mail.oreluniver.ru", 25);
            Console.Write("Type username: ");
            username = Console.ReadLine();
            Console.Write("Type password: ");
            password = Console.ReadLine();

            smtp.Connect();

            if(!smtp.connected){
                Console.WriteLine("Error with connection!");
                return;
            }

            smtp.Login(username, password);

            if(!smtp.authorithation){
                Console.WriteLine("Error with login! Disconnecing...");
                return;
            }

            while(!stop){
                Console.Write("From: ");
                from = Console.ReadLine();
                Console.Write("To: ");
                to = Console.ReadLine();
                Console.Write("\nSubject: ");
                subject = Console.ReadLine();
                Console.Write("\n");
                text = Console.ReadLine();
                
                smtp.CreateEMail(from, to, subject, text);
                smtp.Send();

                if(!smtp.connected)
                    return;

                Console.WriteLine("Do you wand write another letter? y/n");
                while(!questionStop){
                    Console.Write("> ");
                    switch(Console.ReadLine()){
                        case "y":
                            questionStop = true;
                        break;
                        case "n":
                            questionStop = true;
                            stop = true;
                        break;
                        default:
                            Console.WriteLine("Type <y> or <n>");
                        break;
                    }
                }
                questionStop = false;
            }
            smtp.Disconnect();
        }
    }
}
