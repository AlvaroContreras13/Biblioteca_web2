namespace Biblioteca_U2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbusuario")]
    public partial class tbusuario
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbusuario()
        {
            tbcalificacion = new HashSet<tbcalificacion>();
            tbcalificacion1 = new HashSet<tbcalificacion>();
            tblibro = new HashSet<tblibro>();
            tblibro1 = new HashSet<tblibro>();
            tbmovimiento_credito = new HashSet<tbmovimiento_credito>();
            tbmovimiento_credito1 = new HashSet<tbmovimiento_credito>();
            tbprestamo = new HashSet<tbprestamo>();
            tbprestamo1 = new HashSet<tbprestamo>();
            tbprestamo2 = new HashSet<tbprestamo>();
            tbprestamo3 = new HashSet<tbprestamo>();
            tbreserva = new HashSet<tbreserva>();
            tbsolicitud_prestamo = new HashSet<tbsolicitud_prestamo>();
            tbsolicitud_prestamo1 = new HashSet<tbsolicitud_prestamo>();
        }

        [Key]
        public int id_usuario { get; set; }

        [Required]
        [StringLength(100)]
        public string nombre { get; set; }

        [Required]
        [StringLength(100)]
        public string apellido { get; set; }

        [Required]
        [StringLength(100)]
        public string email { get; set; }

        [Required]
        [StringLength(255)]
        public string contrasena { get; set; }

        [Required]
        [StringLength(5)]
        public string codigo_identificacion { get; set; }

        public int id_carrera { get; set; }

        [StringLength(20)]
        public string telefono { get; set; }

        [StringLength(255)]
        public string temas_interes { get; set; }

        public byte[] foto_perfil { get; set; }

        public decimal? reputacion_promedio { get; set; }

        public int? numero_prestamos_completados { get; set; }

        public int? creditos_disponibles { get; set; }

        [StringLength(10)]
        public string rol { get; set; }

        [StringLength(15)]
        public string estado_cuenta { get; set; }

        public bool? activo { get; set; }

        public DateTime? fecha_registro { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbcalificacion> tbcalificacion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbcalificacion> tbcalificacion1 { get; set; }

        public virtual tbcarrera tbcarrera { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblibro> tblibro { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblibro> tblibro1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbmovimiento_credito> tbmovimiento_credito { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbmovimiento_credito> tbmovimiento_credito1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbprestamo> tbprestamo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbprestamo> tbprestamo1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbprestamo> tbprestamo2 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbprestamo> tbprestamo3 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbreserva> tbreserva { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbsolicitud_prestamo> tbsolicitud_prestamo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbsolicitud_prestamo> tbsolicitud_prestamo1 { get; set; }
    }
}
