using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;
using System.Diagnostics;

namespace RaceDay.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApiService _api;

        public HomeController(ILogger<HomeController> logger, ApiService api)
        {
            _logger = logger;
            _api = api;
        }

        public async Task<IActionResult> Index(string? search, string? eventType, string? location, DateTime? fromDate, DateTime? toDate)
        {
            var vm = new HomeVm
            {
                Search = search,
                EventType = eventType,
                Location = location,
                From = fromDate,
                To = toDate
            };

            if (!_api.IsLoggedIn) return View(vm);

            var res = await _api.GetListAsync<EventItem>("api/events");
            if (!res.Ok)
            {
                TempData["Error"] = res.Error;
                return View(vm);
            }

            var events = (res.Data ?? new()).Where(e => e.EventDate.Date >= DateTime.Today);

            if (!string.IsNullOrWhiteSpace(search))
                events = events.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(eventType))
                events = events.Where(e => e.EventType == eventType);
            if (!string.IsNullOrWhiteSpace(location))
                events = events.Where(e => e.Location.Contains(location, StringComparison.OrdinalIgnoreCase));
            if (fromDate.HasValue)
                events = events.Where(e => e.EventDate.Date >= fromDate.Value.Date);
            if (toDate.HasValue)
                events = events.Where(e => e.EventDate.Date <= toDate.Value.Date);

            vm.Events = events.OrderBy(e => e.EventDate).ToList();
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
