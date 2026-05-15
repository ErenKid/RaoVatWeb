using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaoVatWeb.Controllers
{
    [Authorize(Roles = "Vip")]
    public class VipController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}