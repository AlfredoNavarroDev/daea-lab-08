using lab_semana08.Models;

namespace lab_semana08.Repositorios;

public interface ILibroRepositorio
{
    Task<IEnumerable<Libro>> ListarActivosAsync();
    Task<IEnumerable<Libro>> BuscarPorTituloAsync(string titulo);
    Task<Libro?> ObtenerPorIdAsync(int libroId);
    Task<int> InsertarAsync(Libro libro);
    Task ActualizarAsync(Libro libro);
    Task EliminarAsync(int libroId);
}
