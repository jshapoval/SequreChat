using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Messenger.Common.DTOs;
using Messenger.Entities.DTOs;
using Newtonsoft.Json;

namespace Messenger.Common
{
    public class ApiClient
    {
        private readonly string _token;

        public ApiClient(string token)
        {
            _token = token;
        }
                           
        public async Task<ApiCallResult<object>> CancelDialog(int dialogId)
        {
            return await this.CallApi<ApiCallResult<object>>("dialog/cancel",
                new Dictionary<string, string> {{"dialogId", dialogId.ToString()}});
        }

        public async Task<ApiCallResult<DialogDTO>> GetDialog(string email)
        {
            return await this.CallApi<ApiCallResult<DialogDTO>>("dialog",
                "get/" + email);
        }

        public async Task<string> GetAsync(string uri)
        {
            return await Common.GetAsync(uri, x => x.Headers.Add("Authorization", $"Bearer {_token}"));
        }

        public async Task<string> PostAsync(string uri, Dictionary<string, string> paremeters)
        {
            return await Common.PostAsync(uri, paremeters, x => x.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token));
        }

        public async Task<T> CallApi<T>(string method, string queryString)
        {
            return await Call<T>($"{Common.Host}/api/{method}/{queryString}");
        }

        public async Task<T> CallApi<T>(string method, Dictionary<string, string> paremeters)
        {
            return await Call<T>($"{Common.Host}/api/{method}", paremeters);
        }

        public async Task<T> Call<T>(string queryString)
        {
            var response = await GetAsync(queryString);

            return JsonConvert.DeserializeObject<T>(response);
        }

        public async Task<T> Call<T>(string queryString, Dictionary<string, string> paremeters)
        {
            var response = await PostAsync(queryString, paremeters);

            return JsonConvert.DeserializeObject<T>(response);
        }
    }
}
