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

        