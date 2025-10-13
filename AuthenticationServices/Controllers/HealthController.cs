// FICHIER: AuthenticationServices/Controllers/HealthController.cs
// OBJET: Contrôleur minimal pour prouver que l'API démarre (placeholder V1).
// NOTE: L’auth Keycloak et vraies routes arriveront dans les versions suivantes.

using Microsoft.AspNetCore.Mvc;

namespace AuthenticationServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok(new { status = "ok", service = "auth" });
    }
}
