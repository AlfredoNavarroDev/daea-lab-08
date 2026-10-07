using lab_semana08.Models;

namespace lab_semana08.Repositorios;

public interface IPrestamoRepositorio
{
    Task<IEnumerable<PrestamoReporte>> ReportePorFechasAsync(DateTime desde, DateTime hasta);
    Task<int> ContarPendientesPorSocioAsync(int socioId);
    Task<int> ContarPendientesTotalAsync();
    Task<int> CrearAsync(int socioId, IEnumerable<int> libroIds, DateTime fechaPrestamo, DateTime fechaLimite);
    Task DevolverLibroAsync(int prestamoId, int libroId);
}
