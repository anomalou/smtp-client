using System;

namespace smtp_client_csharp
{
    class Program
    {
        static void Main(string[] args)
        {
            // string from, to, subject, text;
            // Console.Write("Type host name: ");
            // hostname = Console.ReadLine();
            // Console.Write("Type host port: ");
            // port = int.Parse(Console.ReadLine());
            SMTP smtp = new SMTP("mail.oreluniver.ru", 25);
            // Console.Write("Type sender address: ");
            // from = Console.ReadLine();
            // Console.Write("Type recepient address: ");
            // to = Console.ReadLine();
            smtp.SetFromAddress("anomalou <cio01@ostu.ru>");
            smtp.SetToAddress("cio01@ostu.ru");
            smtp.Login("cio01", "cio01p");
            // Console.WriteLine("\nEmail\n");
            // Console.Write("Subject: ");
            // subject = Console.ReadLine();
            // Console.WriteLine("\nText\n");
            // text = Console.ReadLine();
            smtp.CreateEMail("Hello", "There");
            smtp.Send();
        }
    }
}
