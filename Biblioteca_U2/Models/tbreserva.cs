namespace Biblioteca_U2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbreserva")]
    public partial class tbreserva
    {
        [Key]
        public int id_reserva { get; set; }

        public int id_libro { get; set; }

        public int id_usuario { get; set; }

        public DateTime? fecha_reserva { get; set; }

        [StringLength(15)]
        public string estado { get; set; }

        public int posicion_cola { get; set; }

        public DateTime? fecha_notificacion { get; set; }

        public DateTime? fecha_expiracion_confirmacion { get; set; }

        public bool? confirmada { get; set; }

        public virtual tblibro tblibro { get; set; }

        public virtual tbusuario tbusuario { get; set; }
    }
}
