using System.ComponentModel.DataAnnotations;

namespace lab_semana08.Models;

public class Socio
{
    public int SocioId { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(15, ErrorMessage = "El DNI no puede superar los 15 caracteres.")]
    [Display(Name = "DNI")]
    public string DNI { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;
}
