using lab_semana08.Models;

namespace lab_semana08.Repositorios;

public interface IAutorRepositorio
{
    Task<IEnumerable<Autor>> ListarActivosAsync();
}
