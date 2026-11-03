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
