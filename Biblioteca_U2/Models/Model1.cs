using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace Biblioteca_U2.Models
{
    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
            this.Configuration.ProxyCreationEnabled = false; 
            this.Configuration.LazyLoadingEnabled = false;  
        }

        public virtual DbSet<tbcalificacion> tbcalificacion { get; set; }
        public virtual DbSet<tbcarrera> tbcarrera { get; set; }
        public virtual DbSet<tbgenero> tbgenero { get; set; }
        public virtual DbSet<tblibro> tblibro { get; set; }
        public virtual DbSet<tbmovimiento_credito> tbmovimiento_credito { get; set; }
        public virtual DbSet<tbprestamo> tbprestamo { get; set; }
        public virtual DbSet<tbreserva> tbreserva { get; set; }
        public virtual DbSet<tbsolicitud_prestamo> tbsolicitud_prestamo { get; set; }
        public virtual DbSet<tbusuario> tbusuario { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<tbcalificacion>()
                .Property(e => e.comentario)
                .IsUnicode(false);

            modelBuilder.Entity<tbcalificacion>()
                .Property(e => e.tipo_calificacion)
                .IsUnicode(false);

            modelBuilder.Entity<tbcarrera>()
                .Property(e => e.nombre_carrera)
                .IsUnicode(false);

            modelBuilder.Entity<tbcarrera>()
                .HasMany(e => e.tbusuario)
                .WithRequired(e => e.tbcarrera)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbgenero>()
                .Property(e => e.nombre_genero)
                .IsUnicode(false);

            modelBuilder.Entity<tbgenero>()
                .HasMany(e => e.tblibro)
                .WithRequired(e => e.tbgenero)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblibro>()
                .Property(e => e.codigo_referencia)
                .IsUnicode(false);

            modelBuilder.Entity<tblibro>()
                .Property(e => e.titulo)
                .IsUnicode(false);

            modelBuilder.Entity<tblibro>()
                .Property(e => e.autor)
                .IsUnicode(false);

            modelBuilder.Entity<tblibro>()
                .Property(e => e.condicion)
                .IsUnicode(false);

            modelBuilder.Entity<tblibro>()
                .Property(e => e.sinopsis)
                .IsUnicode(false);

            modelBuilder.Entity<tblibro>()
                .HasMany(e => e.tbprestamo)
                .WithRequired(e => e.tblibro)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblibro>()
                .HasMany(e => e.tbreserva)
                .WithRequired(e => e.tblibro)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tblibro>()
                .HasMany(e => e.tbsolicitud_prestamo)
                .WithRequired(e => e.tblibro)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbmovimiento_credito>()
                .Property(e => e.tipo_movimiento)
                .IsUnicode(false);

            modelBuilder.Entity<tbmovimiento_credito>()
                .Property(e => e.descripcion)
                .IsUnicode(false);

            modelBuilder.Entity<tbprestamo>()
                .Property(e => e.estado)
                .IsUnicode(false);

            modelBuilder.Entity<tbprestamo>()
                .Property(e => e.condicion_devolucion)
                .IsUnicode(false);

            modelBuilder.Entity<tbprestamo>()
                .Property(e => e.descripcion_danio)
                .IsUnicode(false);

            modelBuilder.Entity<tbprestamo>()
                .HasMany(e => e.tbcalificacion)
                .WithRequired(e => e.tbprestamo)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbreserva>()
                .Property(e => e.estado)
                .IsUnicode(false);

            modelBuilder.Entity<tbsolicitud_prestamo>()
                .Property(e => e.estado)
                .IsUnicode(false);

            modelBuilder.Entity<tbsolicitud_prestamo>()
                .Property(e => e.motivo_rechazo)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.nombre)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.apellido)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.email)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.contrasena)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.codigo_identificacion)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.telefono)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.temas_interes)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.reputacion_promedio)
                .HasPrecision(3, 2);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.rol)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .Property(e => e.estado_cuenta)
                .IsUnicode(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbcalificacion)
                .WithRequired(e => e.tbusuario)
                .HasForeignKey(e => e.id_usuario_calificador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbcalificacion1)
                .WithRequired(e => e.tbusuario1)
                .HasForeignKey(e => e.id_usuario_calificado)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tblibro)
                .WithRequired(e => e.tbusuario)
                .HasForeignKey(e => e.id_admin_registrador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tblibro1)
                .WithRequired(e => e.tbusuario1)
                .HasForeignKey(e => e.id_usuario_donador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbmovimiento_credito)
                .WithOptional(e => e.tbusuario)
                .HasForeignKey(e => e.id_admin);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbmovimiento_credito1)
                .WithRequired(e => e.tbusuario1)
                .HasForeignKey(e => e.id_usuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbprestamo)
                .WithRequired(e => e.tbusuario)
                .HasForeignKey(e => e.id_admin_entrega)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbprestamo1)
                .WithOptional(e => e.tbusuario1)
                .HasForeignKey(e => e.id_admin_devolucion);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbprestamo2)
                .WithRequired(e => e.tbusuario2)
                .HasForeignKey(e => e.id_usuario_prestatario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbprestamo3)
                .WithRequired(e => e.tbusuario3)
                .HasForeignKey(e => e.id_usuario_donador)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbreserva)
                .WithRequired(e => e.tbusuario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbsolicitud_prestamo)
                .WithOptional(e => e.tbusuario)
                .HasForeignKey(e => e.id_admin_procesador);

            modelBuilder.Entity<tbusuario>()
                .HasMany(e => e.tbsolicitud_prestamo1)
                .WithRequired(e => e.tbusuario1)
                .HasForeignKey(e => e.id_usuario_solicitante)
                .WillCascadeOnDelete(false);
        }
    }
}
