using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Biblioteca_U2.Models;
using Newtonsoft.Json.Linq;

namespace Biblioteca_U2.Controllers
{
    public class ChatBotController : Controller
    {
        private readonly Model1 db = new Model1();

        // 🔐 API Key desde Web.config (SEGURO)
        private readonly string apiKey = ConfigurationManager.AppSettings["OpenAI:ApiKey"];
        private readonly string apiModel = ConfigurationManager.AppSettings["OpenAI:Model"];
        private readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        // GET: ChatBot
        public ActionResult ChatBotVirtual()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendMessage(string message)
        {
            string respuesta = "Ocurrió un error al procesar tu mensaje.";

            try
            {
                // 🔍 Obtener contexto del usuario logueado
                int? userId = Session["UserId"] as int?;
                string userName = Session["UserName"] as string ?? "Usuario";
                string userCarrera = Session["UserCarrera"] as string ?? "sin especificar";

                // 📌 Obtener temas de interés del usuario (si existen)
                string temasInteres = "";
                if (userId.HasValue)
                {
                    var usuario = db.tbusuario.Find(userId.Value);
                    if (usuario != null && !string.IsNullOrWhiteSpace(usuario.temas_interes))
                    {
                        temasInteres = usuario.temas_interes;
                    }
                }

                // 📚 Obtener información del catálogo
                string catalogoInfo = ObtenerInformacionCatalogo(message);

                // 🤖 Crear contexto enriquecido para la IA
                string temasInteresInfo = string.IsNullOrWhiteSpace(temasInteres)
                    ? "(No especificado)"
                    : temasInteres;

                string systemPrompt = $@"Eres el asistente virtual de la Biblioteca Universitaria 'BookCycle'. 

INFORMACIÓN DEL USUARIO:
- Nombre: {userName}
- Carrera: {userCarrera}
- Temas de interés: {temasInteresInfo}

CATÁLOGO DISPONIBLE:
{catalogoInfo}

⚠️⚠️⚠️ REGLAS ABSOLUTAS - NO NEGOCIABLES ⚠️⚠️⚠️:

1. 🚫 PROHIBIDO INVENTAR LIBROS
   - NUNCA menciones libros que NO aparezcan en 'CATÁLOGO DISPONIBLE' arriba
   - Si un libro NO está en la lista, di: 'Ese libro no está en nuestro catálogo actual'
   
2. 📋 COPIA EXACTA DE TÍTULOS
   - Cuando recomiendes, COPIA Y PEGA los títulos EXACTOS de 'CATÁLOGO DISPONIBLE'
   - NO parafrasees, NO resumas, NO modifiques los títulos
   
3. 🔍 VERIFICACIÓN OBLIGATORIA
   - Antes de recomendar cualquier libro, VERIFICA que esté en 'CATÁLOGO DISPONIBLE'
   - Si no está en la lista, NO lo menciones
   
4. ❌ CONOCIMIENTO GENERAL DESACTIVADO
   - NO uses tu conocimiento general sobre libros
   - NO sugieras libros famosos que no estén en el catálogo
   - SOLO usa la información de 'CATÁLOGO DISPONIBLE'

EJEMPLO CORRECTO:
Usuario: '¿qué recomiendas?'
Asistente: 'Te recomiendo: Clean Code: A Handbook of Agile Software Craftsmanship por Robert Martin (2008) - Calidad de Software - ✅ Disponible'

EJEMPLO INCORRECTO (PROHIBIDO):
Usuario: '¿qué recomiendas?'
Asistente: 'Te recomiendo: Administración Estratégica por Fred David' ← ❌ ESTE LIBRO NO EXISTE EN EL CATÁLOGO

TU FUNCIÓN:
- Ayudar a buscar libros DEL CATÁLOGO
- Recomendar libros QUE APAREZCAN EN 'CATÁLOGO DISPONIBLE'
- Si el usuario tiene 'Temas de interés', prioriza libros relacionados con esos temas
- Informar disponibilidad SOLO de libros del catálogo
- Explicar cómo solicitar préstamos

INSTRUCCIONES:
- Responde en español, sé amable y conciso
- NO uses formato Markdown (nada de **, ##, - al inicio de línea)
- Usa texto plano simple con emojis si es necesario
- Para solicitar préstamos: deben ir a 'Buscar Libros' en el menú";

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var requestBody = new
                    {
                        model = apiModel,
                        messages = new object[]
                        {
                            new { role = "system", content = systemPrompt },
                            new { role = "user", content = message }
                        },
                        temperature = 0.3,  // Más determinista, menos creativo
                        max_tokens = 800    // Más espacio para respuestas detalladas
                    };

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(apiUrl, content);
                    string responseJson = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var parsed = JObject.Parse(responseJson);
                        respuesta = parsed["choices"]?[0]?["message"]?["content"]?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(respuesta))
                        {
                            respuesta = "No se obtuvo respuesta del asistente.";
                        }
                        else
                        {
                            // 🧹 Limpiar formato Markdown de la respuesta
                            respuesta = LimpiarMarkdown(respuesta);
                        }
                    }
                    else
                    {
                        // Mostrar el error específico de OpenAI
                        var errorObj = JObject.Parse(responseJson);
                        var errorMsg = errorObj["error"]?["message"]?.ToString() ?? "Error desconocido";
                        var errorType = errorObj["error"]?["type"]?.ToString() ?? "";

                        respuesta = $"❌ Error de OpenAI ({response.StatusCode}):\n{errorMsg}\n\n";

                        if (errorType.Contains("invalid_api_key") || errorMsg.Contains("API key"))
                        {
                            respuesta += "🔑 Tu API Key es inválida o ha expirado.\n";
                            respuesta += "👉 Solución:\n";
                            respuesta += "1. Ve a https://platform.openai.com/api-keys\n";
                            respuesta += "2. Crea una nueva API Key\n";
                            respuesta += "3. Actualízala en Web.config (línea OpenAI:ApiKey)";
                        }
                        else if (errorMsg.Contains("quota") || errorMsg.Contains("billing"))
                        {
                            respuesta += "💳 Tu cuenta de OpenAI no tiene créditos o está suspendida.\n";
                            respuesta += "👉 Verifica tu billing en: https://platform.openai.com/account/billing";
                        }
                        else if (errorMsg.Contains("model"))
                        {
                            respuesta += "🤖 El modelo 'gpt-3.5-turbo' no está disponible para tu cuenta.\n";
                            respuesta += "👉 Intenta cambiar el modelo en Web.config a 'gpt-4' o 'gpt-4o-mini'";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = $"Error: {ex.Message}. Verifica que tu API Key esté configurada correctamente.";
            }

            return Json(new { respuesta = respuesta }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Limpia el formato Markdown de la respuesta (**, ##, listas, etc.)
        /// </summary>
        private string LimpiarMarkdown(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            // Remover negritas (**texto** o __texto__)
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"\*\*(.+?)\*\*", "$1");
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"__(.+?)__", "$1");

            // Remover cursivas (*texto* o _texto_)
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"\*(.+?)\*", "$1");
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"_(.+?)_", "$1");

            // Remover encabezados (## texto)
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"^#{1,6}\s+", "", System.Text.RegularExpressions.RegexOptions.Multiline);

            // Remover código inline (`texto`)
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"`(.+?)`", "$1");

            return texto;
        }

        /// <summary>
        /// Obtiene información relevante del catálogo según la consulta del usuario
        /// </summary>
        private string ObtenerInformacionCatalogo(string consulta)
        {
            try
            {
                var sb = new StringBuilder();

                // Convertir a minúsculas para búsqueda case-insensitive
                string consultaLower = consulta?.ToLower() ?? "";

                // Buscar libros relacionados con la consulta (case-insensitive)
                var librosRelacionados = db.tblibro
                    .Where(l => l.titulo.ToLower().Contains(consultaLower) ||
                                l.autor.ToLower().Contains(consultaLower) ||
                                l.tbgenero.nombre_genero.ToLower().Contains(consultaLower))
                    .Take(15)
                    .Select(l => new
                    {
                        l.titulo,
                        l.autor,
                        genero = l.tbgenero.nombre_genero,
                        l.disponible,
                        l.ano_publicacion
                    })
                    .ToList();

                if (librosRelacionados.Any())
                {
                    sb.AppendLine("\n📚 LIBROS RELACIONADOS CON TU BÚSQUEDA:");

                    foreach (var libro in librosRelacionados)
                    {
                        string estado = libro.disponible == true ? "✅ Disponible" : "❌ No disponible";
                        sb.AppendLine($"- '{libro.titulo}' por {libro.autor} ({libro.ano_publicacion}) - {libro.genero} - {estado}");
                    }
                }
                else
                {
                    // Si no hay coincidencias exactas, mostrar TODOS los libros disponibles
                    sb.AppendLine("\n📚 CATÁLOGO COMPLETO DISPONIBLE:");

                    var todosLosLibros = db.tblibro
                        .Where(l => l.disponible == true)
                        .OrderBy(l => l.tbgenero.nombre_genero)
                        .ThenBy(l => l.titulo)
                        .Select(l => new
                        {
                            l.titulo,
                            l.autor,
                            genero = l.tbgenero.nombre_genero,
                            l.disponible,
                            l.ano_publicacion
                        })
                        .ToList();

                    foreach (var libro in todosLosLibros)
                    {
                        sb.AppendLine($"- '{libro.titulo}' por {libro.autor} ({libro.ano_publicacion}) - {libro.genero} - ✅ Disponible");
                    }
                }

                // Agregar resumen
                var totalLibros = db.tblibro.Count();
                var librosDisponibles = db.tblibro.Count(l => l.disponible == true);
                var generosDisponibles = db.tbgenero.Select(g => g.nombre_genero).ToList();

                sb.AppendLine($"\n📊 RESUMEN:");
                sb.AppendLine($"- Total de libros: {totalLibros}");
                sb.AppendLine($"- Disponibles: {librosDisponibles}");
                sb.AppendLine($"- Géneros: {string.Join(", ", generosDisponibles)}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Error al obtener catálogo: {ex.Message}";
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
