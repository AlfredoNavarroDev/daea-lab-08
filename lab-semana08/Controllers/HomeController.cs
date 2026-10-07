using lab_semana08.Models;
using lab_semana08.Repositorios;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace lab_semana08.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILibroRepositorio _libroRepositorio;
        private readonly ISocioRepositorio _socioRepositorio;
        private readonly IPrestamoRepositorio _prestamoRepositorio;

        public HomeController(
            ILibroRepositorio libroRepositorio,
            ISocioRepositorio socioRepositorio,
            IPrestamoRepositorio prestamoRepositorio)
        {
            _libroRepositorio = libroRepositorio;
            _socioRepositorio = socioRepositorio;
            _prestamoRepositorio = prestamoRepositorio;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Inicio";

            var libros = await _libroRepositorio.ListarActivosAsync();
            var socios = await _socioRepositorio.ListarActivosAsync();

            ViewBag.LibrosActivos = libros.Count();
            ViewBag.SociosActivos = socios.Count();
            ViewBag.PrestamosPendientes = await _prestamoRepositorio.ContarPendientesTotalAsync();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
