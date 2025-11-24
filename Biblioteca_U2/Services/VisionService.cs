using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Biblioteca_U2.Services
{
    public class VisionService
    {
        private static readonly string apiKey = "apikey inserta vision";

        public static async Task<InfoLibro> AnalizarLibro(byte[] imageBytes)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                string base64 = Convert.ToBase64String(imageBytes);

                var body = new
                {
                    model = "gpt-4o",
                    messages = new object[]
                    {
                        new {
                            role = "system",
                            content = "Eres un asistente experto en catalogación de libros. " +
                                      "Tu tarea es identificar el libro que aparece en la portada y devolver UNA SINOPSIS REAL basada en información bibliográfica, " +
                                      "NO una descripción visual de la portada. " +
                                      "IMPORTANTE: El campo \"anio\" SOLO debe tener un valor si aparece de forma explícita en la portada " +
                                      "(por ejemplo números como 2014, 2020, 1999). " +
                                      "SI EL AÑO NO APARECE, debes dejar \"anio\": 0. " +
                                      "NO inventes el año, NO lo deduzcas, NO uses conocimientos previos ni memoria del modelo.\n" +
                                      "Devuelve SOLO un JSON válido con este formato exacto: " +
                                      "{ " +
                                      "\"titulo\": \"...\", " +
                                      "\"autor\": \"...\", " +
                                      "\"categoria\": \"...\", " +
                                      "\"anio\": 0, " +
                                      "\"descripcion\": \"(una sinopsis del contenido del libro, NO descripción visual)\" " +
                                      "} " +
                                      "Si no reconoces el libro, genera una sinopsis lógica del tema basado en el título y autor, " +
                                      "pero nunca inventes un año de publicación."
                        },
                        new {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "text", text = "Analiza esta portada de libro" },
                                new {
                                    type = "image_url",
                                    image_url = new {
                                        url = "data:image/jpeg;base64," + base64
                                    }
                                }
                            }
                        }
                    }
                };

                string json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                string raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception(raw);

                dynamic result = JsonConvert.DeserializeObject(raw);

                string texto;

                var contentNode = result.choices[0].message.content;

                // ✔ CASO 1: content es string
                if (contentNode is Newtonsoft.Json.Linq.JValue)
                {
                    texto = contentNode.ToString();
                }
                // ✔ CASO 2: content es array
                else
                {
                    texto = contentNode[0].text.ToString();
                }

                // Limpieza opcional
                texto = texto.Trim();
                
                // Remover posibles marcadores de código markdown
                if (texto.StartsWith("```json"))
                {
                    texto = texto.Substring(7);
                }
                if (texto.StartsWith("```"))
                {
                    texto = texto.Substring(3);
                }
                if (texto.EndsWith("```"))
                {
                    texto = texto.Substring(0, texto.Length - 3);
                }
                texto = texto.Trim();

                return JsonConvert.DeserializeObject<InfoLibro>(texto);
            }
        }

        public static async Task<string> ClasificarCarreraDinamico(string titulo, string descripcion, string categoria, List<string> carreras)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", apiKey);

                // Convertimos la lista a un string separado por saltos
                string listaCarreras = string.Join("\n", carreras);

                string prompt =
                    "Te voy a dar información sobre un libro. Tu tarea es analizarlo y elegir SOLO una carrera de la lista proporcionada.\n" +
                    "Debes responder únicamente con el nombre EXACTO de la carrera que mejor se relacione.\n\n" +

                    "LISTA DE CARRERAS DISPONIBLES:\n" +
                    listaCarreras +
                    "\n\n" +

                    "INFORMACIÓN DEL LIBRO:\n" +
                    "Título: " + titulo + "\n" +
                    "Categoría: " + categoria + "\n" +
                    "Descripción: " + descripcion + "\n\n" +
                    "RESPONDE SOLO con una carrera exacta de la lista y nada más.";

                var body = new
                {
                    model = "gpt-4o",
                    messages = new[] {
                        new { role = "user", content = prompt }
                    }
                };

                string json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                string raw = await response.Content.ReadAsStringAsync();

                dynamic result = JsonConvert.DeserializeObject(raw);
                string carrera = result.choices[0].message.content.ToString().Trim();

                return carrera;
            }
        }
    }

    public class InfoLibro
    {
        [JsonProperty("titulo")]
        public string Titulo { get; set; }

        [JsonProperty("autor")]
        public string Autor { get; set; }

        [JsonProperty("categoria")]
        public string Categoria { get; set; }

        [JsonProperty("anio")]
        public int? Anio { get; set; }

        [JsonProperty("descripcion")]
        public string Descripcion { get; set; }
    }
}
