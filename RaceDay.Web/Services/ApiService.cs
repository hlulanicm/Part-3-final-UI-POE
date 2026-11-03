using System.Net.Http.Json;

namespace RaceDay.Web.Services
{
    public class ApiService
    {
       
      private readonly HttpClient _http;
       private readonly IHttpContextAccessor _ctx;
     private  ISession Session =>  _ctx.HttpContext!.Session;

      public ApiService (HttpClient http, IHttpContextAccessor ctx) { _http =  http; _ctx  = ctx; }

       public bool IsLoggedIn => Session.GetString("ApiCookie") != null;

        public string? Role => Session.GetString("Role");

            public string? UserName => Session.GetString("UserName");


        private HttpRequestMessage Build(HttpMethod method, string url, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);
            var cookie = Session.GetString("ApiCookie");

            if (cookie != null) req.Headers.Add("Cookie", cookie);
             if (body != null) req.Content = JsonContent.Create(body);
            return req;
        }



        public async Task<(bool ok, string error)> LoginAsync(string email, string password)
        {

           var res = await _http.PostAsJsonAsync("api/Auth/login", new { email, password });
        
            if (!res.IsSuccessStatusCode) return (false, await res.Content.ReadAsStringAsync());

            var setCookie = res.Headers.GetValues("Set-Cookie").First(c => c.StartsWith("RaceDayAuth"));
            Session.SetString("ApiCookie", setCookie.Split(';')[0]);

            var user = await res.Content.ReadFromJsonAsync<LoginResult>();


             Session.SetString("Role", user!.Role);
            Session.SetString("UserName", user.UserName);
            return (true, "");



        }






        public async Task<HttpResponseMessage> RegisterAsync(object dto) =>
               await _http.PostAsJsonAsync("api/Auth/register", dto);


        public async Task LogoutAsync()
        {
            await _http.SendAsync(Build(HttpMethod.Post, "api/Auth/logout"));
            Session.Clear();

        }

        public Task<HttpResponseMessage> GetAsync(string url) => _http.SendAsync(Build(HttpMethod.Get, url));
        public Task<HttpResponseMessage> PostAsync(string url, object body) => _http.SendAsync(Build(HttpMethod.Post, url, body));
        public Task<HttpResponseMessage> PutAsync(string url, object body) => _http.SendAsync(Build(HttpMethod.Put, url, body));
        public Task<HttpResponseMessage> DeleteAsync(string url) => _http.SendAsync(Build(HttpMethod.Delete, url));
    }

    public record LoginResult(int UserID, string UserName, string Role);
}




