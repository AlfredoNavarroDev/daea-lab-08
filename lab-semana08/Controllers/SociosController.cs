using lab_semana08.Models;
using lab_semana08.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace lab_semana08.Controllers;

public class SociosController : Controller
{
    private readonly ISocioRepositorio _socioRepositorio;

    public SociosController(ISocioRepositorio socioRepositorio)
    {
        _socioRepositorio = socioRepositorio;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Socios";
        var socios = await _socioRepositorio.ListarActivosAsync();
        return View(socios);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Nuevo socio";
        return View(new Socio());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Socio socio)
    {
        ViewData["Title"] = "Nuevo socio";

        if (ModelState.IsValid)
        {
            var existente = await _socioRepositorio.ObtenerPorDNIAsync(socio.DNI);
            if (existente is not null)
            {
                ModelState.AddModelError(nameof(Socio.DNI), "Ya existe un socio registrado con ese DNI.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(socio);
        }

        await _socioRepositorio.InsertarAsync(socio);
        TempData["Mensaje"] = $"Socio \"{socio.Nombre}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
