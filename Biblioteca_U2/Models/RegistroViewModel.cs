using System.ComponentModel.DataAnnotations;

namespace Biblioteca_U2.Models
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es requerido")]
        [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; }

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@virtual\.upt\.pe$",
            ErrorMessage = "Debe usar su correo institucional @virtual.upt.pe")]
        [Display(Name = "Correo Institucional UPT")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; }

        [Required(ErrorMessage = "Confirma tu contraseña")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar Contraseña")]
        [Compare("Contrasena", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmarContrasena { get; set; }

        [Required(ErrorMessage = "El código de identificación es requerido")]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "El código debe tener exactamente 5 caracteres")]
        [Display(Name = "Código de Identificación")]
        [RegularExpression(@"^[0-9]{5}$", ErrorMessage = "El código debe contener 5 dígitos")]
        public string CodigoIdentificacion { get; set; }

        [Required(ErrorMessage = "Selecciona tu carrera")]
        [Display(Name = "Carrera")]
        public int IdCarrera { get; set; }

        [Phone(ErrorMessage = "Número de teléfono inválido")]
        [Display(Name = "Teléfono (opcional)")]
        public string Telefono { get; set; }

        [Required(ErrorMessage = "Debes aceptar los términos y condiciones")]
        [Display(Name = "Acepto los términos y condiciones")]
        public bool AceptarTerminos { get; set; }
    }
}