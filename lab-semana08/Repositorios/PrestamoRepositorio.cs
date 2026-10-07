using System.Data;
using Dapper;
using lab_semana08.Models;
using Microsoft.Data.SqlClient;

namespace lab_semana08.Repositorios;

public class PrestamoRepositorio : IPrestamoRepositorio
{
    private readonly string _connectionString;

    public PrestamoRepositorio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaDB'.");
    }

    private SqlConnection CrearConexion() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<PrestamoReporte>> ReportePorFechasAsync(DateTime desde, DateTime hasta)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<PrestamoReporte>(
            "dbo.sp_Prestamos_ReportePorFechas",
            new { Desde = desde, Hasta = hasta },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ContarPendientesPorSocioAsync(int socioId)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(
            "dbo.sp_Prestamos_ContarPendientesPorSocio",
            new { SocioId = socioId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> ContarPendientesTotalAsync()
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(
            "dbo.sp_Prestamos_ContarPendientesTotal",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> CrearAsync(int socioId, IEnumerable<int> libroIds, DateTime fechaPrestamo, DateTime fechaLimite)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync();
        using var transaccion = conexion.BeginTransaction();

        try
        {
            var prestamoId = await conexion.ExecuteScalarAsync<int>(
                "dbo.sp_Prestamos_Insertar",
                new { SocioId = socioId, FechaPrestamo = fechaPrestamo, FechaLimite = fechaLimite },
                transaccion,
                commandType: CommandType.StoredProcedure);

            foreach (var libroId in libroIds)
            {
                await conexion.ExecuteAsync(
                    "dbo.sp_DetallePrestamo_Insertar",
                    new { PrestamoId = prestamoId, LibroId = libroId },
                    transaccion,
                    commandType: CommandType.StoredProcedure);

                await conexion.ExecuteAsync(
                    "dbo.sp_Libros_DecrementarEjemplares",
                    new { LibroId = libroId },
                    transaccion,
                    commandType: CommandType.StoredProcedure);
            }

            transaccion.Commit();
            return prestamoId;
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }

    public async Task DevolverLibroAsync(int prestamoId, int libroId)
    {
        using var conexion = CrearConexion();
        await conexion.OpenAsync();
        using var transaccion = conexion.BeginTransaction();

        try
        {
            await conexion.ExecuteAsync(
                "dbo.sp_DetallePrestamo_MarcarDevuelto",
                new { PrestamoId = prestamoId, LibroId = libroId },
                transaccion,
                commandType: CommandType.StoredProcedure);

            await conexion.ExecuteAsync(
                "dbo.sp_Libros_IncrementarEjemplares",
                new { LibroId = libroId },
                transaccion,
                commandType: CommandType.StoredProcedure);

            var pendientes = await conexion.ExecuteScalarAsync<int>(
                "dbo.sp_Prestamos_ContarPendientesPorPrestamo",
                new { PrestamoId = prestamoId },
                transaccion,
                commandType: CommandType.StoredProcedure);

            if (pendientes == 0)
            {
                await conexion.ExecuteAsync(
                    "dbo.sp_Prestamos_ActualizarEstado",
                    new { PrestamoId = prestamoId, Estado = "Devuelto" },
                    transaccion,
                    commandType: CommandType.StoredProcedure);
            }

            transaccion.Commit();
        }
        catch
        {
            transaccion.Rollback();
            throw;
        }
    }
}
