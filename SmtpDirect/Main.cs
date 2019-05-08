//https://ru.stackoverflow.com/questions/976992/Как-отправить-отчёт-себе-на-почту-в-windows-forms-без-ввода-пароля-c
//
// *** SmtpDirect example: Sending e-mail directly into the destination SMTP server ***

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mail;
using SMTP;

namespace ConsoleTest1
{
    class Program
    {       
        static void Main(string[] args)
        {
            string dest = "pupkin@example.com";
            string[] elems = dest.Split("@".ToCharArray());
            string[] arr = Mx.GetMXRecords(elems[1]);

            string s = "";
            foreach (string x in arr) s += x + Environment.NewLine;
            Console.WriteLine("MX records:" + Environment.NewLine + s);

            MailMessage msg = new MailMessage();
            msg.To = dest;
            msg.From = "pupkin@example.com";
            msg.Subject = "Hello, world";
            msg.Body = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
            SmtpDirect.SmtpServer = arr[0];

            SmtpDirect.Send(msg);
            Console.WriteLine(SmtpDirect.output.ToString());

            Console.ReadKey();
        }

    }
}
