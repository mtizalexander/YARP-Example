using Microsoft.AspNetCore.Mvc;

namespace SM.ProximaCentauri.Controllers;

public class AttractionsController : Controller
{
    public IActionResult ProximaB() => View();

    public IActionResult FlareWatch() => View();

    public IActionResult EmberMarket() => View();
}
