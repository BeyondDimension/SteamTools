using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Application.Models;

namespace System.Application.Services
{
    public interface IProjectService
    {
        Task<NoticeType[]> GetProjectNoticeAsync();
        Task<ActivitiesType[]> GetActivitiesAsync();
        Task<ListItemDataType[]> GetFakeListAsync(int count = 0);
        Task<NoticeItem[]> GetNoticesAsync();
    }

    public class ProjectService : IProjectService
    {
        private readonly HttpClient _httpClient;

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NoticeType[]> GetProjectNoticeAsync()
        {
            return await _httpClient.GetFromJsonAsync<NoticeType[]>("data/notice.json");
        }

        public async Task<NoticeItem[]> GetNoticesAsync()
        {
            return await _httpClient.GetFromJsonAsync<NoticeItem[]>("data/notices.json");
        }

        public async Task<ActivitiesType[]> GetActivitiesAsync()
        {
            return await _httpClient.GetFromJsonAsync<ActivitiesType[]>("data/activities.json");
        }

        public async Task<ListItemDataType[]> GetFakeListAsync(int count = 0)
        {
            var data = await _httpClient.GetFromJsonAsync<ListItemDataType[]>("data/fake_list.json");
            return count > 0 ? data.Take(count).ToArray() : data;
        }
    }
}