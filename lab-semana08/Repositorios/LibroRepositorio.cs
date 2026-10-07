using System.Data;
using Dapper;
using lab_semana08.Models;
using Microsoft.Data.SqlClient;

namespace lab_semana08.Repositorios;

public class LibroRepositorio : ILibroRepositorio
{
    private readonly string _connectionString;

    public LibroRepositorio(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("BibliotecaDB")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaDB'.");
    }

    private SqlConnection CrearConexion() => new SqlConnection(_connectionString);

    public async Task<IEnumerable<Libro>> ListarActivosAsync()
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<Libro>(
            "dbo.sp_Libros_ListarActivos",
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Libro>> BuscarPorTituloAsync(string titulo)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryAsync<Libro>(
            "dbo.sp_Libros_BuscarPorTitulo",
            new { Titulo = titulo },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<Libro?> ObtenerPorIdAsync(int libroId)
    {
        using var conexion = CrearConexion();
        return await conexion.QueryFirstOrDefaultAsync<Libro>(
            "dbo.sp_Libros_ObtenerPorId",
            new { LibroId = libroId },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<int> InsertarAsync(Libro libro)
    {
        using var conexion = CrearConexion();
        return await conexion.ExecuteScalarAsync<int>(
            "dbo.sp_Libros_Insertar",
            new { libro.Titulo, libro.ISBN, libro.AutorId, libro.Ejemplares },
            commandType: CommandType.StoredProcedure);
    }

    public async Task ActualizarAsync(Libro libro)
    {
        using var conexion = CrearConexion();
        await conexion.ExecuteAsync(
            "dbo.sp_Libros_Actualizar",
            new { libro.LibroId, libro.Titulo, libro.ISBN, libro.AutorId, libro.Ejemplares },
            commandType: CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(int libroId)
    {
        using var conexion = CrearConexion();
        await conexion.ExecuteAsync(
            "dbo.sp_Libros_Eliminar",
            new { LibroId = libroId },
            commandType: CommandType.StoredProcedure);
    }
}
