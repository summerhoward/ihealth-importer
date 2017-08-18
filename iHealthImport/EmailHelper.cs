using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace iHealthImport
{
    public class EmailHelper
    {
        private static readonly string ErrorEmailAddess = ConfigurationManager.AppSettings["ErrorEmailAddess"];

        public static void SendEmailAsync()
        {
            SmtpClient smtp = new SmtpClient("smtp.office365.com")
            {
                EnableSsl = true,
                Port = 587,
                Credentials = new NetworkCredential("mypoindexter@mypoindexter.com", "Mailbox1")
            };
            smtp.Send("mypoindexter@mypoindexter.com", ErrorEmailAddess, "iHealth Import Error", "Please check log.");
        }
    }
}