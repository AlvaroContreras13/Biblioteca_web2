namespace Biblioteca_U2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbmovimiento_credito
    {
        [Key]
        public int id_movimiento { get; set; }

        public int id_usuario { get; set; }

        [Required]
        [StringLength(10)]
        public string tipo_movimiento { get; set; }

        public int cantidad { get; set; }

        public int? saldo_anterior { get; set; }

        public int? saldo_nuevo { get; set; }

        [StringLength(200)]
        public string descripcion { get; set; }

        public int? id_admin { get; set; }

        public DateTime? fecha_movimiento { get; set; }

        public virtual tbusuario tbusuario { get; set; }

        public virtual tbusuario tbusuario1 { get; set; }
    }
}
