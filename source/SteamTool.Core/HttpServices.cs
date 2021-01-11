using Newtonsoft.Json;
using Ninject.Modules;
using SteamTool.Core.Common;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SteamTool.Core
{
    public class HttpServices
    {
        private readonly HttpClient _client = new HttpClient();

        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";

        public string Accept { get; set; } = "application/json";

        private async Task<string> SendAsync(HttpRequestMessage request)
        {
            try
            {
                var response = await _client.SendAsync(request).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }
        private async Task<Stream> SendAsyncAsStream(HttpRequestMessage request)
        {
            try
            {
                var response = await _client.SendAsync(request).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return null;
        }

        public async Task<string> Get(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("Accept", Accept);
            return await SendAsync(request);
        }
        public async Task<string> Get(string url, Dictionary<string, string> keyValuePairs)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            foreach (var item in keyValuePairs)
            {
                request.Headers.Add(item.Key, item.Value);
            }
            if (!keyValuePairs.ContainsKey("User-Agent"))
            {
                request.Headers.Add("User-Agent", UserAgent);
            }
            if (!keyValuePairs.ContainsKey("Accept"))
            {
                request.Headers.Add("Accept", Accept);
            }
            return await SendAsync(request);
        }
        public async Task<Stream> GetStream(string url, Dictionary<string, string> keyValuePairs)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            foreach (var item in keyValuePairs)
            {
                request.Headers.Add(item.Key, item.Value);
            }
            if (!keyValuePairs.ContainsKey("User-Agent"))
            {
                request.Headers.Add("User-Agent", UserAgent);
            }
            if (!keyValuePairs.ContainsKey("Accept"))
            {
                request.Headers.Add("Accept", Accept);
            }

            return await SendAsyncAsStream(request);
        }

        public async Task<string> Post(string url, string content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(content)
            };
            request.Headers.Add("User-Agent", UserAgent);
            request.Headers.Add("Accept", Accept);
            return await SendAsync(request);
        }
        public async Task<string> Post(string url, Dictionary<string, string> keyValuePairs, string content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(content)
            };
            foreach (var item in keyValuePairs)
            {
                request.Headers.Add(item.Key, item.Value);
            }
            if (!keyValuePairs.ContainsKey("User-Agent"))
            {
                request.Headers.Add("User-Agent", UserAgent);
            }
            if (!keyValuePairs.ContainsKey("Accept"))
            {
                request.Headers.Add("Accept", Accept);
            }

            return await SendAsync(request);
        }
        public async Task<Stream> PostStream(string url, Dictionary<string, string> keyValuePairs, Stream content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StreamContent(content)
            };
            foreach (var item in keyValuePairs)
            {
                request.Headers.Add(item.Key, item.Value);
            }
            if (!keyValuePairs.ContainsKey("User-Agent"))
            {
                request.Headers.Add("User-Agent", UserAgent);
            }
            if (!keyValuePairs.ContainsKey("Accept"))
            {
                request.Headers.Add("Accept", Accept);
            }

            return await SendAsyncAsStream(request);
        }
    }
}
