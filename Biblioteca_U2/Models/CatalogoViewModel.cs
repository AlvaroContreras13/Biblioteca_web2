using System.Collections.Generic;
using System.Web.Mvc;

namespace Biblioteca_U2.Models
{
    public class CatalogoViewModel
    {
        public string Busqueda { get; set; }
        public int? IdGenero { get; set; }
        public int? IdCarrera { get; set; }
        public bool? Disponible { get; set; }
        public IEnumerable<SelectListItem> Generos { get; set; }
        public IEnumerable<SelectListItem> Carreras { get; set; }

        public List<tblibro> Libros { get; set; }
        public string UsuarioNombre { get; set; }

        // 🔢 Paginación
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalResultados { get; set; }
    }
}
