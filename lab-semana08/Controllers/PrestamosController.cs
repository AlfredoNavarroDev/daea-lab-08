using lab_semana08.Models;
using lab_semana08.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace lab_semana08.Controllers;

public class PrestamosController : Controller
{
    private readonly IPrestamoRepositorio _prestamoRepositorio;
    private readonly ISocioRepositorio _socioRepositorio;
    private readonly ILibroRepositorio _libroRepositorio;

    public PrestamosController(
        IPrestamoRepositorio prestamoRepositorio,
        ISocioRepositorio socioRepositorio,
        ILibroRepositorio libroRepositorio)
    {
        _prestamoRepositorio = prestamoRepositorio;
        _socioRepositorio = socioRepositorio;
        _libroRepositorio = libroRepositorio;
    }

    public async Task<IActionResult> Reporte(DateTime? desde, DateTime? hasta)
    {
        ViewData["Title"] = "Reporte de préstamos";

        var hoy = DateTime.Today;
        var fechaDesde = desde ?? hoy.AddMonths(-1);
        var fechaHasta = hasta ?? hoy;

        ViewData["Desde"] = fechaDesde.ToString("yyyy-MM-dd");
        ViewData["Hasta"] = fechaHasta.ToString("yyyy-MM-dd");

        var reporte = await _prestamoRepositorio.ReportePorFechasAsync(fechaDesde, fechaHasta);
        return View(reporte);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Devolver(int prestamoId, int libroId, string? desde, string? hasta)
    {
        await _prestamoRepositorio.DevolverLibroAsync(prestamoId, libroId);
        TempData["Mensaje"] = "Devolución registrada correctamente.";
        return RedirectToAction(nameof(Reporte), new { desde, hasta });
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nuevo préstamo";
        await CargarDatosAsync();
        return View(new PrestamoCrearViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PrestamoCrearViewModel modelo)
    {
        ViewData["Title"] = "Nuevo préstamo";

        if (modelo.LibroIds.Count == 0)
        {
            ModelState.AddModelError(nameof(modelo.LibroIds), "Seleccione al menos un libro.");
        }

        if (ModelState.IsValid)
        {
            var pendientes = await _prestamoRepositorio.ContarPendientesPorSocioAsync(modelo.SocioId);
            if (pendientes + modelo.LibroIds.Count > 3)
            {
                ModelState.AddModelError(nameof(modelo.SocioId),
                    $"El socio ya tiene {pendientes} libro(s) pendiente(s) de devolución; no puede superar un máximo de 3 libros pendientes a la vez.");
            }
        }

        if (!ModelState.IsValid)
        {
            await CargarDatosAsync();
            return View(modelo);
        }

        var hoy = DateTime.Today;
        await _prestamoRepositorio.CrearAsync(modelo.SocioId, modelo.LibroIds, hoy, hoy.AddDays(14));
        TempData["Mensaje"] = "Préstamo registrado correctamente.";
        return RedirectToAction(nameof(Reporte));
    }

    private async Task CargarDatosAsync()
    {
        var socios = await _socioRepositorio.ListarActivosAsync();
        ViewBag.Socios = new SelectList(socios, "SocioId", "Nombre");

        var libros = await _libroRepositorio.ListarActivosAsync();
        ViewBag.LibrosDisponibles = libros.Where(l => l.Ejemplares > 0).ToList();
    }
}
