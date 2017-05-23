using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace FEA_Ultil
{
  public  class FEASendMail
  {
      public static void SendMailMessage(string to, string bcc, string cc, string subject, string body)
      {
          MailMessage mMailMessage = new MailMessage();
          mMailMessage.From = new MailAddress("webmaster@feavn.com.vn");

          mMailMessage.To.Add(to);

         
          if ((bcc != null) && (bcc != string.Empty))
          {
              mMailMessage.Bcc.Add(new MailAddress(bcc));
          }
          if ((cc != null) && (cc != string.Empty))
          {
              mMailMessage.CC.Add(new MailAddress(cc));
          }
          mMailMessage.Subject = subject;
          mMailMessage.Body = body;
          mMailMessage.IsBodyHtml = true;
          mMailMessage.Priority = MailPriority.Normal;
          SmtpClient mSmtpClient = new SmtpClient();
          mSmtpClient.Send(mMailMessage);
      }

      public static void SendMailMessage2(string to, string bcc, string cc, string subject, string bodyMsg)
      {
          MailMessage mMailMessage = new MailMessage();

          mMailMessage.From = new MailAddress("webmaster@feavn.com.vn");
          mMailMessage.To.Add(new MailAddress(to));

          if ((bcc != null) && (bcc != string.Empty))
          {
              mMailMessage.Bcc.Add(new MailAddress(bcc));
          }
          if ((cc != null) && (cc != string.Empty))
          {
              mMailMessage.CC.Add(new MailAddress(cc));
          }
          mMailMessage.Subject = subject;
          mMailMessage.Body = bodyMsg.ToString();
          mMailMessage.IsBodyHtml = true;
          mMailMessage.Priority = MailPriority.Normal;
          SmtpClient mSmtpClient = new SmtpClient();
          mSmtpClient.Send(mMailMessage);
      }
    }
}
