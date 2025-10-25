namespace Biblioteca_U2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbcalificacion")]
    public partial class tbcalificacion
    {
        [Key]
        public int id_calificacion { get; set; }

        public int id_prestamo { get; set; }

        public int id_usuario_calificador { get; set; }

        public int id_usuario_calificado { get; set; }

        public int puntuacion { get; set; }

        [Column(TypeName = "text")]
        public string comentario { get; set; }

        [Required]
        [StringLength(30)]
        public string tipo_calificacion { get; set; }

        public DateTime? fecha_calificacion { get; set; }

        public virtual tbprestamo tbprestamo { get; set; }

        public virtual tbusuario tbusuario { get; set; }

        public virtual tbusuario tbusuario1 { get; set; }
    }
}
