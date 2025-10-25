namespace Biblioteca_U2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbprestamo")]
    public partial class tbprestamo
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbprestamo()
        {
            tbcalificacion = new HashSet<tbcalificacion>();
        }

        [Key]
        public int id_prestamo { get; set; }

        public int id_libro { get; set; }

        public int id_usuario_prestatario { get; set; }

        public int id_usuario_donador { get; set; }

        public int id_admin_entrega { get; set; }

        public int? id_admin_devolucion { get; set; }

        public DateTime? fecha_entrega { get; set; }

        public DateTime fecha_prevista_devolucion { get; set; }

        public DateTime? fecha_devolucion_real { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int? dias_prestamo { get; set; }

        [StringLength(15)]
        public string estado { get; set; }

        [StringLength(10)]
        public string condicion_devolucion { get; set; }

        [Column(TypeName = "text")]
        public string descripcion_danio { get; set; }

        public int? numero_renovaciones { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbcalificacion> tbcalificacion { get; set; }

        public virtual tblibro tblibro { get; set; }

        public virtual tbusuario tbusuario { get; set; }

        public virtual tbusuario tbusuario1 { get; set; }

        public virtual tbusuario tbusuario2 { get; set; }

        public virtual tbusuario tbusuario3 { get; set; }
    }
}
