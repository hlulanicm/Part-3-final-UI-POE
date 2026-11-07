using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.Web.Services
{
    // Outcome of an API call without data
    public class ApiResult
    {
        public bool Ok { get; init; }
        public string Error { get; init; } = "";
        public HttpStatusCode Status { get; init; }
    }

    // Outcome of an API call that returns data
    public class ApiResult<T> : ApiResult
    {
        public T? Data { get; init; }
    }

    // The only class in the MVC project that talks to the Part 2 API
    public class ApiService
    {
        private readonly HttpClient _http;
         private readonly IHttpContextAccessor _ctx;

        private ISession Session => _ctx.HttpContext!.Session;

        public ApiService(HttpClient http, IHttpContextAccessor ctx) { _http = http; _ctx = ctx; }

        // Login state, role and name are kept in the session


        public bool IsLoggedIn => Session.GetString("ApiCookie") != null;
        public string? Role => Session.GetString("Role");
        public string? UserName => Session.GetString("UserName");
        public int? UserId => Session.GetInt32("UserID");
        public bool IsOrganiser => Role == "Organiser";
        public bool IsParticipant => Role == "Participant";

        public void SetUserName(string name) => Session.SetString("UserName", name);

        // Builds a request and adds the API cookie from the session
        private HttpRequestMessage Build(HttpMethod method, string url, object? body = null)
        {
            var req = new HttpRequestMessage(method, url);
            var cookie = Session.GetString("ApiCookie");

            if (cookie != null) req.Headers.Add("Cookie", cookie);
            if (body != null) req.Content = JsonContent.Create(body);
            return req;
        }

        // Sends the request and keeps the API cookie in the session up to date
        private async Task<HttpResponseMessage> SendRaw(HttpRequestMessage req)
        {
            var res = await _http.SendAsync(req);

            if (res.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var fresh = cookies.FirstOrDefault(c => c.StartsWith("RaceDayAuth="));
                if (fresh != null)
                {
                    var value = fresh.Split(';')[0];
                    if (value.Length > "RaceDayAuth=".Length)
                        Session.SetString("ApiCookie", value);
                }
            }

            // An expired API session also ends the MVC session
            if (res.StatusCode == HttpStatusCode.Unauthorized && IsLoggedIn)
                Session.Clear();

            return res;
        }

        // Turns an API error response into a short readable message
        private static async Task<string> ReadError(HttpResponseMessage res)
        {
            var text = (await res.Content.ReadAsStringAsync()).Trim().Trim('"');

            // Validation errors arrive as a JSON object with an "errors" list
            if (text.StartsWith("{"))
            {
                try
                {
                    using var doc = JsonDocument.Parse(text);
                    if (doc.RootElement.TryGetProperty("errors", out var errors))
                    {
                        var messages = errors.EnumerateObject()
                            .SelectMany(p => p.Value.EnumerateArray().Select(m => m.GetString()))
                            .Where(m => !string.IsNullOrWhiteSpace(m));
                        var joined = string.Join(" ", messages);
                        if (joined.Length > 0) return joined;
                    }
                }
                catch (JsonException) { }
                return "Some of the details you entered are not valid.";
            }

            if (!string.IsNullOrWhiteSpace(text) && (int)res.StatusCode < 500) return text;

            return res.StatusCode switch
            {
                HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
                HttpStatusCode.Forbidden => "You do not have permission to do that.",
                HttpStatusCode.NotFound => "That item could not be found.",
                _ => "Something went wrong. Please try again."
            };
        }

    private static ApiResult Unavailable() => new()
        {
            Ok = false,
            Status = HttpStatusCode.ServiceUnavailable,
            Error = "The RaceDay service is not available right now. Please try again shortly." //display a suitable error message when the system is down 



        };

        private static ApiResult<T> Unavailable<T>() => new()
        {
            Ok = false,
            Status = HttpStatusCode.ServiceUnavailable,
            Error = "The RaceDay service is not available right now. Please try again shortly."
        };

        // ---------- Login, register, logout ----------

        public async Task<(bool ok, string error)> LoginAsync(string email, string password)
        {
            try
            {
                using var res = await _http.PostAsJsonAsync("api/Auth/login", new { email, password });

                if (!res.IsSuccessStatusCode)
                    return (false, await ReadError(res));

                // Keep the API cookie in the session
                var setCookie = res.Headers.GetValues("Set-Cookie").First(c => c.StartsWith("RaceDayAuth"));
                Session.SetString("ApiCookie", setCookie.Split(';')[0]);

                var user = await res.Content.ReadFromJsonAsync<LoginResult>();
                Session.SetString("Role", user!.Role);
                Session.SetString("UserName", user.UserName);
                Session.SetInt32("UserID", user.UserID);
                return (true, "");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, Unavailable().Error);
            }
        }

        public async Task<ApiResult> RegisterAsync(object dto)
        {
            try
            {
                using var res = await _http.PostAsJsonAsync("api/Auth/register", dto);
                return res.IsSuccessStatusCode
                    ? new ApiResult { Ok = true, Status = res.StatusCode }
                    : new ApiResult { Ok = false, Status = res.StatusCode, Error = await ReadError(res) };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Unavailable();
            }
        }

        public async Task LogoutAsync()
        {
            try { using var res = await SendRaw(Build(HttpMethod.Post, "api/Auth/logout")); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { }
            Session.Clear();
        }

        // ---------- Generic calls ----------

        // Sends a request that returns no data
        public async Task<ApiResult> SendAsync(HttpMethod method, string url, object? body = null)
        {
            try
            {
                using var res = await SendRaw(Build(method, url, body));
                return res.IsSuccessStatusCode
                    ? new ApiResult { Ok = true, Status = res.StatusCode }
                    : new ApiResult { Ok = false, Status = res.StatusCode, Error = await ReadError(res) };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Unavailable();
            }
        }

        // Sends a request and reads the JSON reply into T
        public async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null)
        {
            try
            {
                using var res = await SendRaw(Build(method, url, body));
                if (!res.IsSuccessStatusCode)
                    return new ApiResult<T> { Ok = false, Status = res.StatusCode, Error = await ReadError(res) };

                var data = await res.Content.ReadFromJsonAsync<T>();
                return new ApiResult<T> { Ok = true, Status = res.StatusCode, Data = data };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Unavailable<T>();
            }
            catch (JsonException)
            {
                return new ApiResult<T> { Ok = false, Status = HttpStatusCode.BadGateway, Error = "The RaceDay service sent an unexpected reply." };
            }
        }

        public Task<ApiResult<T>> GetAsync<T>(string url) => SendAsync<T>(HttpMethod.Get, url);
        public Task<ApiResult> PostAsync(string url, object body) => SendAsync(HttpMethod.Post, url, body);
        public Task<ApiResult<T>> PostAsync<T>(string url, object body) => SendAsync<T>(HttpMethod.Post, url, body);
        public Task<ApiResult> PutAsync(string url, object body) => SendAsync(HttpMethod.Put, url, body);
        public Task<ApiResult<T>> PutAsync<T>(string url, object body) => SendAsync<T>(HttpMethod.Put, url, body);
        public Task<ApiResult> DeleteAsync(string url) => SendAsync(HttpMethod.Delete, url);

        // Gets a list and treats "404 not found" as an empty list
        public async Task<ApiResult<List<T>>> GetListAsync<T>(string url)
        {
            var res = await GetAsync<List<T>>(url);
            if (res.Status == HttpStatusCode.NotFound)
                return new ApiResult<List<T>> { Ok = true, Status = res.Status, Data = new List<T>() };
            if (res.Ok && res.Data == null)
                return new ApiResult<List<T>> { Ok = true, Status = res.Status, Data = new List<T>() };
            return res;
        }

        // Sends an image to the API, which stores it in Azure Blob Storage

        public async Task<ApiResult> PostFileAsync(string url, IFormFile file)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var stream = file.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.FileName);

                var req = Build(HttpMethod.Post, url);
                req.Content = content;

                using var res = await SendRaw(req);
                return res.IsSuccessStatusCode
                    ? new ApiResult { Ok = true, Status = res.StatusCode }
                    : new ApiResult { Ok = false, Status = res.StatusCode, Error = await ReadError(res) };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return Unavailable();
            }
        }
    }

    public record LoginResult(int UserID, string UserName, string Role);
}
