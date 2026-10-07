using System.ComponentModel.DataAnnotations;

namespace lab_semana08.Models;

public class PrestamoCrearViewModel
{
    [Required(ErrorMessage = "Seleccione un socio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un socio.")]
    [Display(Name = "Socio")]
    public int SocioId { get; set; }

    [Display(Name = "Libros")]
    public List<int> LibroIds { get; set; } = new();
}
