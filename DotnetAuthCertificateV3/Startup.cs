using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace DotnetAuthCertificateV3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme) // Ou outro tipo de autenticação desejado
                 .AddCookie(options =>
                 {
                     options.LoginPath = "/Login"; // Página de login para usuários não autenticados
                 });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseMiddleware<CertificateAuthenticationMiddleware>(); // Captura e valida o certificado


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class CertificateAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public CertificateAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var clientCertificate = await context.Connection.GetClientCertificateAsync();

            if (clientCertificate != null && ValidateCertificate(clientCertificate))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, clientCertificate.Subject),  // Nome do certificado
                    new Claim("Thumbprint", clientCertificate.Thumbprint)  // Hash do certificado
                };

                var identity = new ClaimsIdentity(claims, "Certificate");
                context.User = new ClaimsPrincipal(identity);
            }

            await _next(context);
        }

        private bool ValidateCertificate(X509Certificate2 certificate)
        {
            return certificate.NotAfter > DateTime.UtcNow && certificate.NotBefore < DateTime.UtcNow;
        }
    }
}
