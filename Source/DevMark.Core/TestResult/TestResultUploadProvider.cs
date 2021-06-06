using DevMark.Model;
using DevMark.Model.Service;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DevMark.Core.TestResult
{
    public partial class TestResultUploadProvider
    {
        private readonly string _url;
        private readonly string _apiToken;
        private readonly ILogger<TestResultUploadProvider> _logger;
        private readonly JsonSerializerSettings _settings;

        public TestResultUploadProvider(ILogger<TestResultUploadProvider> logger, JsonSerializerSettings jsonSettings, string url, string apiToken)
        {
            _url = url;
            _apiToken = apiToken;
            _logger = logger;
            _settings = jsonSettings;
        }

        public async Task<UploadResult> Upload(TestRun testRun)
        {
            
            string content = JsonConvert.SerializeObject(new PostResultsRequest { TestRun = testRun }, _settings);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_url);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", _apiToken);
                    var respMessage = await client.PostAsync("api/v1/Results", new StringContent(content, Encoding.Default, "application/json"));

                    var status = respMessage.StatusCode;

                    if (status == HttpStatusCode.OK || status == HttpStatusCode.BadRequest)
                    {
                        var respString = await respMessage.Content.ReadAsStringAsync();
                        var response = JsonConvert.DeserializeObject<PostResultResponse>(respString);
                        return new UploadResult { Success = status == HttpStatusCode.OK, Message = response.Message, Url = response.Url };
                    }
                    else if (status == HttpStatusCode.Unauthorized)
                    {
                        return new UploadResult { Success = false, Message = $"Authentication failed, please check your API token at \"{_url}\"." };
                    }
                    else
                    {
                        await TryLogError(respMessage);
                        return new UploadResult { Success = false, Message = "Upload failed." };
                    }
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Upload failed: {e.Message}");
                return new UploadResult { Success = false };
            }
        }

        private async Task TryLogError(HttpResponseMessage respMessage)
        {
            string content = null;
            try
            {
                content = await respMessage.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {

            }
            string contentMessage = "";
            if (content != null)
            {
                contentMessage = $" Message: {content}";
            }
            _logger.LogError($"Upload failed with status code {respMessage.StatusCode}.{contentMessage}");
        }
    }
}
