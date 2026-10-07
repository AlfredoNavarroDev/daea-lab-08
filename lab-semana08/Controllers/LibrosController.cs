using lab_semana08.Models;
using lab_semana08.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace lab_semana08.Controllers;

public class LibrosController : Controller
{
    private readonly ILibroRepositorio _libroRepositorio;
    private readonly IAutorRepositorio _autorRepositorio;

    public LibrosController(ILibroRepositorio libroRepositorio, IAutorRepositorio autorRepositorio)
    {
        _libroRepositorio = libroRepositorio;
        _autorRepositorio = autorRepositorio;
    }

    public async Task<IActionResult> Index(string? titulo)
    {
        ViewData["Title"] = "Libros";
        ViewData["Titulo"] = titulo;

        var libros = string.IsNullOrWhiteSpace(titulo)
            ? await _libroRepositorio.ListarActivosAsync()
            : await _libroRepositorio.BuscarPorTituloAsync(titulo);

        return View(libros);
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Detalle del libro";

        var libro = await _libroRepositorio.ObtenerPorIdAsync(id);
        if (libro is null)
        {
            return NotFound();
        }

        return View(libro);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Nuevo libro";
        await CargarAutoresAsync();
        return View(new Libro());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Libro libro)
    {
        ViewData["Title"] = "Nuevo libro";

        if (!ModelState.IsValid)
        {
            await CargarAutoresAsync();
            return View(libro);
        }

        await _libroRepositorio.InsertarAsync(libro);
        TempData["Mensaje"] = $"Libro \"{libro.Titulo}\" creado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Editar libro";

        var libro = await _libroRepositorio.ObtenerPorIdAsync(id);
        if (libro is null)
        {
            return NotFound();
        }

        await CargarAutoresAsync();
        return View(libro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Libro libro)
    {
        ViewData["Title"] = "Editar libro";

        if (id != libro.LibroId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await CargarAutoresAsync();
            return View(libro);
        }

        await _libroRepositorio.ActualizarAsync(libro);
        TempData["Mensaje"] = $"Libro \"{libro.Titulo}\" actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        ViewData["Title"] = "Eliminar libro";

        var libro = await _libroRepositorio.ObtenerPorIdAsync(id);
        if (libro is null)
        {
            return NotFound();
        }

        return View(libro);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var libro = await _libroRepositorio.ObtenerPorIdAsync(id);
        await _libroRepositorio.EliminarAsync(id);
        TempData["Mensaje"] = libro is null
            ? "Libro eliminado correctamente."
            : $"Libro \"{libro.Titulo}\" eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarAutoresAsync()
    {
        var autores = await _autorRepositorio.ListarActivosAsync();
        ViewBag.Autores = new SelectList(autores, "AutorId", "Nombre");
    }
}
