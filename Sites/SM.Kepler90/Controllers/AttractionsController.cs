using Microsoft.AspNetCore.Mvc;

namespace SM.Kepler90.Controllers;

public class AttractionsController : Controller
{
    public IActionResult Kepler90h() => View();

    public IActionResult OrreryRail() => View();

    public IActionResult FurnaceBeach() => View();
}
