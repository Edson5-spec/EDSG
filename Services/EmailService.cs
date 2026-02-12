using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EDSG.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                _logger.LogInformation("Enviando email para: {Email}", toEmail);

                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password);
                    client.EnableSsl = _emailSettings.EnableSsl;
                    client.Timeout = 10000; // 10 segundos

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);

                    _logger.LogInformation("Email enviado com sucesso para: {Email}", toEmail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar email para {Email}", toEmail);
                throw new Exception($"Erro ao enviar email: {ex.Message}", ex);
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Bem-vindo à Plataforma EDSG! 🎉";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #007bff, #6610f2); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; background: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>Bem-vindo à EDSG!</h1>
                        <p>Ecossistema Digital de Serviços Geridos</p>
                    </div>
                    
                    <div class='content'>
                        <h2>Olá, {userName}!</h2>
                        <p>Estamos muito felizes por ter você conosco! Sua conta foi criada com sucesso na Plataforma EDSG.</p>
                        
                        <h3>🎯 O que você pode fazer agora:</h3>
                        <ul>
                            <li>📋 <strong>Criar seu perfil profissional</strong> - Destaque suas habilidades e experiência</li>
                            <li>🔍 <strong>Procurar serviços</strong> - Encontre profissionais qualificados</li>
                            <li>💼 <strong>Oferecer serviços</strong> - Mostre seu trabalho e conquiste clientes</li>
                            <li>⭐ <strong>Construir sua reputação</strong> - Receba avaliações dos seus clientes</li>
                        </ul>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='https://localhost:5001/Dashboard' class='button'>COMEÇAR AGORA</a>
                        </div>
                        
                        <h3>📞 Precisa de ajuda?</h3>
                        <p>Nossa equipe de suporte está sempre disponível para ajudá-lo:</p>
                        <ul>
                            <li>📧 Email: support@edsg.com</li>
                            <li>🕒 Horário: Seg-Sex, 9h-18h</li>
                        </ul>
                    </div>
                    
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} EDSG - Ecossistema Digital de Serviços Geridos</p>
                        <p>Esta é uma mensagem automática, por favor não responda a este email.</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var subject = "🔐 Redefinir sua Palavra-passe - EDSG";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #dc3545, #fd7e14); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; background: #dc3545; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
                        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h2>Redefinir Palavra-passe</h2>
                    </div>
                    
                    <div class='content'>
                        <p>Olá,</p>
                        <p>Recebemos uma solicitação para redefinir a palavra-passe da sua conta EDSG.</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' class='button'>REDEFINIR PALAVRA-PASSE</a>
                        </div>
                        
                        <p>Se o botão não funcionar, copie e cole o seguinte link no seu navegador:</p>
                        <p style='background: #e9ecef; padding: 10px; border-radius: 5px; word-break: break-all;'>
                            {resetLink}
                        </p>
                        
                        <div class='warning'>
                            <strong>⚠️ Importante:</strong>
                            <ul>
                                <li>Este link expirará em 24 horas</li>
                                <li>Se não foi você que solicitou esta alteração, ignore este email</li>
                                <li>Nunca compartilhe este link com ninguém</li>
                            </ul>
                        </div>
                        
                        <p><strong>Dicas de segurança:</strong></p>
                        <ul>
                            <li>Use uma palavra-passe única e forte</li>
                            <li>Não reutilize palavras-passe de outros sites</li>
                            <li>Considere usar um gerenciador de palavras-passe</li>
                        </ul>
                    </div>
                    
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} EDSG - Ecossistema Digital de Serviços Geridos</p>
                        <p>Esta é uma mensagem automática de segurança.</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAccountConfirmationEmailAsync(string toEmail, string confirmationLink)
        {
            var subject = "✅ Confirme sua Conta - EDSG";
            var body = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #28a745, #20c997); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; background: #28a745; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; font-weight: bold; }}
                        .footer {{ text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; color: #666; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h2>Confirme sua Conta</h2>
                    </div>
                    
                    <div class='content'>
                        <p>Olá,</p>
                        <p>Obrigado por se registrar na Plataforma EDSG! Para ativar sua conta, precisamos que você confirme seu endereço de email.</p>
                        
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{confirmationLink}' class='button'>CONFIRMAR MINHA CONTA</a>
                        </div>
                        
                        <p>Se o botão não funcionar, copie e cole o seguinte link no seu navegador:</p>
                        <p style='background: #e9ecef; padding: 10px; border-radius: 5px; word-break: break-all;'>
                            {confirmationLink}
                        </p>
                        
                        <p><strong>Por que confirmar sua conta?</strong></p>
                        <ul>
                            <li>🌐 Acesso completo à plataforma</li>
                            <li>🔒 Maior segurança para sua conta</li>
                            <li>📱 Recebimento de notificações importantes</li>
                            <li>💼 Capacidade de oferecer serviços profissionais</li>
                        </ul>
                    </div>
                    
                    <div class='footer'>
                        <p>© {DateTime.Now.Year} EDSG - Ecossistema Digital de Serviços Geridos</p>
                        <p>Esta é uma mensagem automática de confirmação de conta.</p>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SenderName { get; set; } = "EDSG Plataforma";
        public string SenderEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = true;
    }
}