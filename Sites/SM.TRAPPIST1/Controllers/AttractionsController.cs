using Microsoft.AspNetCore.Mvc;

namespace SM.TRAPPIST1.Controllers;

public class AttractionsController : Controller
{
    public IActionResult Trappist1e() => View();

    public IActionResult ResonanceCruise() => View();

    public IActionResult SevenWorldsSkyway() => View();
}
