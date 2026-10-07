using System.ComponentModel.DataAnnotations;

namespace lab_semana08.Models;

public class Libro
{
    public int LibroId { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ISBN es obligatorio.")]
    [StringLength(20, ErrorMessage = "El ISBN no puede superar los 20 caracteres.")]
    [Display(Name = "ISBN")]
    public string ISBN { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione un autor.")]
    [Display(Name = "Autor")]
    public int AutorId { get; set; }

    public string? AutorNombre { get; set; }

    [Range(0, 1000, ErrorMessage = "Los ejemplares deben estar entre 0 y 1000.")]
    public int Ejemplares { get; set; }

    public bool Activo { get; set; } = true;
}
