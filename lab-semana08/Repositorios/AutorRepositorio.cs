using System.Data;
using Dapper;
using lab_semana08.Models;
using Microsoft.Data.SqlClient;

namespace lab_semana08.Repositorios;

public class AutorRepositorio : IAutorRepositorio
{
    private readonly string _connectionString;

    public AutorRepositorio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaDB'.");
    }

    private SqlConnection CrearConexion() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<Autor>> ListarActivosAsync()
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<Autor>(
            "dbo.sp_Autores_ListarActivos",
            commandType: CommandType.StoredProcedure);
    }
}
