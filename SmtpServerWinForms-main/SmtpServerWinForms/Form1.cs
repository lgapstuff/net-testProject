using MailKit.Net.Smtp;
using MimeKit;
using SmtpServer;
using SmtpServer.Storage;
using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace SmtpServerWinForms
{
    public partial class Form1 : Form
    {
        private SmtpServer.SmtpServer _smtpServer;

        public Form1()
        {
            InitializeComponent();
        }


        public class SampleMessageStore : MessageStore
        {
            public override Task<SmtpServer.Protocol.SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
            {
                var message = Encoding.UTF8.GetString(buffer.ToArray());

                MessageBox.Show("Received message:\n" + message);

                return Task.FromResult(SmtpServer.Protocol.SmtpResponse.Ok);
            }
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            var options = new SmtpServerOptionsBuilder()
            .ServerName("localhost")
            .Port(25, 587) // SMTP ports
            .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IMessageStore, SampleMessageStore>()
                .BuildServiceProvider();

            _smtpServer = new SmtpServer.SmtpServer(options, serviceProvider);

            var serverTask = _smtpServer.StartAsync(CancellationToken.None);

            MessageBox.Show("SMTP server is running...");
        }

        private void btnSendEmail_Click(object sender, EventArgs e)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Sender Name", txtFrom.Text));
            message.To.Add(new MailboxAddress("Receiver Name", txtTo.Text));
            message.Subject = txtSubject.Text;

            message.Body = new TextPart("plain")
            {
                Text = txtBody.Text
            };

            using (var client = new SmtpClient())
            {
                client.Connect("localhost", 25, false);

                // Si el servidor requiere autenticación, descomente y configure lo siguiente:
                // client.Authenticate("username", "password");

                client.Send(message);
                client.Disconnect(true);
            }

            MessageBox.Show("Email sent successfully!");
        }
    }
}
