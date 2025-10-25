namespace Biblioteca_U2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbsolicitud_prestamo
    {
        [Key]
        public int id_solicitud { get; set; }

        public int id_libro { get; set; }

        public int id_usuario_solicitante { get; set; }

        public int? id_admin_procesador { get; set; }

        [StringLength(10)]
        public string estado { get; set; }

        public DateTime? fecha_solicitud { get; set; }

        public DateTime? fecha_procesamiento { get; set; }

        [StringLength(200)]
        public string motivo_rechazo { get; set; }

        public virtual tblibro tblibro { get; set; }

        public virtual tbusuario tbusuario { get; set; }

        public virtual tbusuario tbusuario1 { get; set; }
    }
}
