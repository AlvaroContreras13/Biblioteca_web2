namespace Biblioteca_U2.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("tbcarrera")]
    public partial class tbcarrera
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbcarrera()
        {
            tblibro = new HashSet<tblibro>();
            tbusuario = new HashSet<tbusuario>();
        }

        [Key]
        public int id_carrera { get; set; }

        [Required]
        [StringLength(150)]
        public string nombre_carrera { get; set; }

        public bool? activa { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblibro> tblibro { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbusuario> tbusuario { get; set; }
    }
}
