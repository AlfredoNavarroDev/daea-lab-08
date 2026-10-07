using lab_semana08.Models;

namespace lab_semana08.Repositorios;

public interface ISocioRepositorio
{
    Task<IEnumerable<Socio>> ListarActivosAsync();
    Task<Socio?> ObtenerPorDNIAsync(string dni);
    Task<int> InsertarAsync(Socio socio);
}
