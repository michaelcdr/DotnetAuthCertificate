using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using System.Security.Authentication;

namespace DotnetAuthCertificateV3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.ConfigureHttpsDefaults(httpsOptions =>
                    {
                        httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate; // Permite certificados
                        httpsOptions.SslProtocols = SslProtocols.Tls12; // Usa TLS seguro
                    });
                })
                .UseStartup<Startup>()
                .Build();
    }
}
