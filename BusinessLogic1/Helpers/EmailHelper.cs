using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace BusinessLogic.Helpers
{
    public class EmailHelper : IDisposable
    {
        protected System.Net.Mail.MailMessage Message;
        protected System.Net.Mail.SmtpClient Smtp;
        protected System.Net.NetworkCredential NetworkCredential;
        protected string _Subject;
        protected string _Body;
        protected string _Recipient;
        public byte[] _File;
        public string FileName;
        public string FileMediaType;
        public string _From;
        public string _CCs;
        public List<BusinessLogic.BusinessObjects.EmailAttachment> Attachemnts { get; set; }

        public EmailHelper(Guid? CompanyID = null)
        {
            using (PanelLogic bll = new PanelLogic())
            {
              

                var mail = new
                {
                    Host = "in-v3.mailjet.com",
                    Port = 587,
                    User = "33c439ffb95efcbc5485db2b7d7b2a8d",
                    Password = "c79cf142ad93773c8373bba9c75d93de",
                    From = "luka1996gacic@gmail.com",
                    Timeout = 100000
                };

                Smtp = new System.Net.Mail.SmtpClient(mail.Host, mail.Port) { EnableSsl = true };
                NetworkCredential = new System.Net.NetworkCredential(mail.User, mail.Password);
                Smtp.Credentials = NetworkCredential;
                Smtp.Timeout = mail.Timeout;
                _From = mail.From;
            }
        }

        public string Subject
        {
            get
            {
                return _Subject;
            }
            set
            {
                _Subject = value;
            }
        }

        public string Body
        {
            get
            {
                return _Body;
            }
            set
            {
                _Body = value;
            }
        }

        public string From
        {
            get
            {
                return _From;
            }
            set
            {
                _From = value;
            }
        }

        public string Recipient
        {
            get
            {
                return _Recipient;
            }
            set
            {
                _Recipient = value;
                //_Recipient = "luka@axelyos.com";
            }
        }

        public string CCs
        {
            get
            {
                return _CCs;
            }
            set
            {
                _CCs = value;
            }
        }

        public byte[] File
        {
            get
            {
                return _File;
            }
            set
            {
                _File = value;
            }
        }

        public void Send()
        {
            if (_Subject.Length > 0 && _Body.Length > 0 && _Recipient.Length > 0)
            {
                Message = new System.Net.Mail.MailMessage(_From, _Recipient) { Subject = _Subject };

                if (Attachemnts != null)
                {
                    try
                    {
                        foreach (var Attachemnt in Attachemnts)
                        {
                            Message.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(Attachemnt.Content), Attachemnt.FileName + Attachemnt.Extension, Attachemnt.MediaType));

                        }

                    }
                    catch
                    {

                    }
                }

                if (_File != null)
                {
                    try
                    {
                        Message.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(_File), FileName, FileMediaType));
                    }
                    catch
                    {

                    }
                }

                if (!string.IsNullOrEmpty(FileName))
                {
                    try
                    {
                        string[] separators = { ",", " " };
                        string[] FileNames = FileName.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in FileNames)
                            Message.Attachments.Add(new System.Net.Mail.Attachment(item, FileMediaType));
                    }
                    catch
                    {

                    }
                }

                if (!string.IsNullOrEmpty(CCs))
                {
                    var ccList = CCs.Split(',').ToList();
                    if (ccList.Count() > 0)
                    {
                        foreach (var cc in ccList)
                        {
                            if (!string.IsNullOrEmpty(cc))
                                Message.CC.Add(new System.Net.Mail.MailAddress(cc));
                        }
                    }
                }


                Message.BodyEncoding = System.Text.Encoding.UTF8;
                Message.IsBodyHtml = true;

                Message.Body = _Body;

                try
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    Smtp.Send(Message);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new Exception("Email must contain subject, body and receipient.");
            }
        }

        void IDisposable.Dispose()
        {
            Message = null;
            Smtp = null;
            NetworkCredential = null;
            _Body = null;
            _File = null;
            FileName = null;
            FileMediaType = null;
            _Recipient = null;
            _Subject = null;
            _From = null;
        }
    }
}
