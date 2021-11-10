using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SIO_AgendaWPF.Models;

namespace SIO_AgendaWPF.Repositories
{
    interface IAgendaRepository
    {
        public Task<List<Devoir>> GetDevoirs();
        public Task<Devoir> GetDevoir(int id);
        public Task<int> PostDevoirs(Devoir devoirs);
        public Task<bool> UpdateDevoirs(int id, Devoir devoirs);
        public Task<bool> RestoreDevoir();
        public Task<bool> DeleteDevoirs(int id);

        public Task<List<Classe>> GetClasses();
        public Task<Classe> GetClasse(int id);

        public Task<List<Matiere>> GetMatieres();
        public Task<Matiere> GetMatiere(int id);
    }

    class AgendaRepository : IAgendaRepository
    {
        public async Task<List<Devoir>> GetDevoirs()
        {
            string reponseJson;
            using (var client = new HttpClient())
            {
                var reponse = await client.GetAsync(new Uri("https://madsioagenda.alwaysdata.net/api/devoirs"));
                reponseJson = await reponse.Content.ReadAsStringAsync();
                if (!reponse.IsSuccessStatusCode)
                {
                    return null;
                }
            }
            return JsonConvert.DeserializeObject<List<Devoir>>(reponseJson);
        }
        public async Task<Devoir> GetDevoir(int id)
        {
            string reponseJson;
            using (var client = new HttpClient())
            {
                var reponse = await client.GetAsync(new Uri($"https://madsioagenda.alwaysdata.net/api/devoirs/{id}"));
                reponseJson = await reponse.Content.ReadAsStringAsync();
                if (!reponse.IsSuccessStatusCode)
                {
                    return null;
                }
            }
            return JsonConvert.DeserializeObject<Devoir>(reponseJson);
        }
        public async Task<int> PostDevoirs(Devoir devoir)
        {
            using (var client = new HttpClient())
            {
                var keyValuesContent = new Dictionary<string, string>()
                {
                    { "Libelle", devoir.Libelle },
                    { "Matiere", devoir.Matiere.Id.ToString() },
                    { "Classe", devoir.Classe.Id.ToString() },
                    { "Date", $"{devoir.Date.Year}-{devoir.Date.Month}-{devoir.Date.Day}" },
                    { "Description", devoir.Description }
                };
                var content = new FormUrlEncodedContent(keyValuesContent);
                var reponse = await client.PostAsync(new Uri("https://madsioagenda.alwaysdata.net/api/devoirs"), content);

                if (reponse.IsSuccessStatusCode)
                {
                    return int.Parse(await reponse.Content.ReadAsStringAsync());
                }
                return -1;
            }
        }
        public async Task<bool> UpdateDevoirs(int id, Devoir devoir)
        {
            using (var client = new HttpClient())
            {
                var keyValuesContent = new Dictionary<string, string>()
                {
                    { "Libelle", devoir.Libelle },
                    { "Matiere", devoir.Matiere.Id.ToString() },
                    { "Classe", devoir.Classe.Id.ToString() },
                    { "Date", $"{devoir.Date.Year}-{devoir.Date.Month}-{devoir.Date.Day}" },
                    { "Description", devoir.Description }
                };
                var content = new FormUrlEncodedContent(keyValuesContent);
                var reponse = await client.PutAsync(new Uri($"https://madsioagenda.alwaysdata.net/api/devoirs/{id}"), content);

                return reponse.IsSuccessStatusCode;
            }
        }
        public async Task<bool> RestoreDevoir()
        {
            string reponseJson;
            using (var client = new HttpClient())
            {
                var reponse = await client.PatchAsync(new Uri($"https://madsioagenda.alwaysdata.net/api/devoirs"), null);
                reponseJson = await reponse.Content.ReadAsStringAsync();
                return reponse.IsSuccessStatusCode;
            }
        }
        public async Task<bool> DeleteDevoirs(int id)
        {
            using (var client = new HttpClient())
            {
                var reponse = await client.DeleteAsync(new Uri($"https://madsioagenda.alwaysdata.net/api/devoirs/{id}"));
                return reponse.IsSuccessStatusCode;
            }
        }

        public async Task<List<Classe>> GetClasses()
        {
            string reponseJson;
            using (var client = new HttpClient())
            {
                var reponse = await client.GetAsync(new Uri("https://madsioagenda.alwaysdata.net/api/classes"));
                reponseJson = await reponse.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<List<Classe>>(reponseJson);
        }
        public async Task<Classe> GetClasse(int id)
        {
            string reponseJson;
            using (var client = new HttpClient())
            {
                var reponse = await client.GetAsync(new Uri($"https://madsioagenda.alwaysdata.net/api/classes/{id}"));
                reponseJson = await reponse.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<Classe>(reponseJson);
        }

        public async Task<List<Matiere>> GetMatieres()
        {
            string reponseJson;
            using (var client = new HttpClient())
            {
                var reponse = await client.GetAsync(new Uri($"https://madsioagenda.alwaysdata.net/api/matieres"));
                reponseJson = await reponse.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<List<Matiere>>(reponseJson);
        }
        public async Task<Matiere> GetMatiere(int id)
        {
            string reponseJson;
            using (var client = new HttpClient())
            {
                var reponse = await client.GetAsync(new Uri($"https://madsioagenda.alwaysdata.net/api/matieres/{id}"));
                reponseJson = await reponse.Content.ReadAsStringAsync();
            }
            return JsonConvert.DeserializeObject<Matiere>(reponseJson);
        }
    }
}
