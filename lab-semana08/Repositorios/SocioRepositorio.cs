using System.Data;
using Dapper;
using lab_semana08.Models;
using Microsoft.Data.SqlClient;

namespace lab_semana08.Repositorios;

public class SocioRepositorio : ISocioRepositorio
{
    private readonly string _connectionString;

    public SocioRepositorio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaDB'.");
    }

    private SqlConnection CrearConexion() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<Socio>> ListarActivosAsync()
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<Socio>(
            "dbo.sp_Socios_ListarActivos",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Socio?> ObtenerPorDNIAsync(string dni)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryFirstOrDefaultAsync<Socio>(
            "dbo.sp_Socios_ObtenerPorDNI",
            new { DNI = dni },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InsertarAsync(Socio socio)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(
            "dbo.sp_Socios_Insertar",
            new { socio.DNI, socio.Nombre, socio.Email },
            commandType: CommandType.StoredProcedure);
    }
}
