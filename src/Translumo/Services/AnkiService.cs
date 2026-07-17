using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Translumo.Services
{
    public class AnkiService
    {
        private readonly HttpClient client;
        private readonly string url = "http://127.0.0.1:8765";

        public AnkiService()
        {
            client = new HttpClient();
        }

        public async Task AddBasicNoteAsync(string front, string back, string deckName = "Default", string modelName = "Basic")
        {
            var payload = new
            {
                action = "addNote",
                version = 6,
                @params = new
                {
                    note = new
                    {
                        deckName = deckName,
                        modelName = modelName,
                        fields = new { Front = front, Back = back },
                        tags = new string[] { "translumo" }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var resp = await client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
        }
    }
}
