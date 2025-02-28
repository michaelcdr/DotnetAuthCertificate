using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System;

namespace DotnetAuthCertificateV3.Controllers
{
    public class CertificadoController : Controller
    {

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ValidarCertificado()
        {
            var clientCertificate = await Request.HttpContext.Connection.GetClientCertificateAsync();

            if (clientCertificate != null && ValidarCertificado(clientCertificate))
            {
                // Certificado válido - autenticar o usuário
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, clientCertificate.Subject),      // Nome do certificado
                    new Claim("Thumbprint", clientCertificate.Thumbprint)       // Thumbprint do certificado
                };

                var identity = new ClaimsIdentity(claims, "Certificate");
                var principal = new ClaimsPrincipal(identity);
                HttpContext.User = principal;                                   // Define o usuário autenticado

                return Json("CertificadoValido");                               // Página de sucesso ou próxima ação
            }

            // Caso o certificado seja inválido ou ausente
            return Json("CertificadoInvalido");  // Página de erro ou ação
        }

        private bool ValidarCertificado(X509Certificate2 certificate)
        {
            // Validação simples - pode ser expandida conforme necessidade
            return certificate.NotAfter > DateTime.UtcNow && certificate.NotBefore < DateTime.UtcNow;
        }
    }
}
