using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace Biblioteca_U2.Controllers
{
    public class ChatBotController : Controller
    {
        // 🔑 Pega tu clave de OpenAI aquí
        private readonly string apiKey = "inserta api";
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
                using (var client = new HttpClient())
                {
                    // Encabezado con la clave de API
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    // Cuerpo del mensaje (modelo ChatGPT)
                    var requestBody = new
                    {
                        model = "gpt-3.5-turbo",
                        messages = new object[]
                        {
                            new { role = "system", content = "Eres el asistente virtual de la biblioteca BookCicle. Responde siempre en español, de manera amable y clara." },
                            new { role = "user", content = message }
                        }
                    };

                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Enviamos la solicitud HTTP
                    var response = await client.PostAsync(apiUrl, content);
                    string responseJson = await response.Content.ReadAsStringAsync();

                    // Leemos la respuesta JSON
                    var parsed = JObject.Parse(responseJson);
                    respuesta = parsed["choices"]?[0]?["message"]?["content"]?.ToString()?.Trim();

                    if (string.IsNullOrEmpty(respuesta))
                    {
                        respuesta = "No se obtuvo respuesta del asistente.";
                    }
                }
            }
            catch (Exception ex)
            {
                respuesta = "Error: " + ex.Message;
            }

            // Retornamos JSON para mostrar en la vista
            return Json(new { respuesta = respuesta }, JsonRequestBehavior.AllowGet);
        }
    }
}
