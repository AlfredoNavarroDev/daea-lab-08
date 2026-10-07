namespace lab_semana08.Models;

public class PrestamoReporte
{
    public int PrestamoId { get; set; }
    public int LibroId { get; set; }
    public string SocioNombre { get; set; } = string.Empty;
    public string LibroTitulo { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
    public DateTime FechaPrestamo { get; set; }

    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
    public DateTime FechaLimite { get; set; }

    [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
    public DateTime? FechaDevolucion { get; set; }

    public string Estado { get; set; } = string.Empty;
}
