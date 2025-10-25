namespace Biblioteca_U2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tblibro")]
    public partial class tblibro
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblibro()
        {
            tbprestamo = new HashSet<tbprestamo>();
            tbreserva = new HashSet<tbreserva>();
            tbsolicitud_prestamo = new HashSet<tbsolicitud_prestamo>();
        }

        [Key]
        public int id_libro { get; set; }

        [Required]
        [StringLength(20)]
        public string codigo_referencia { get; set; }

        public int id_usuario_donador { get; set; }

        public int id_admin_registrador { get; set; }

        [Required]
        [StringLength(150)]
        public string titulo { get; set; }

        [Required]
        [StringLength(100)]
        public string autor { get; set; }

        public int id_genero { get; set; }

        public int? id_carrera { get; set; }

        [StringLength(10)]
        public string condicion { get; set; }

        public int? ano_publicacion { get; set; }

        [Column(TypeName = "text")]
        public string sinopsis { get; set; }

        public bool? disponible { get; set; }

        public DateTime? fecha_donacion { get; set; }

        public virtual tbcarrera tbcarrera { get; set; }

        public virtual tbgenero tbgenero { get; set; }

        public virtual tbusuario tbusuario { get; set; }

        public virtual tbusuario tbusuario1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbprestamo> tbprestamo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbreserva> tbreserva { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbsolicitud_prestamo> tbsolicitud_prestamo { get; set; }
    }
}
