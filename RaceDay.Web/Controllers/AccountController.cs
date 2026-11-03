using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;
        // Connects our API service

        public AccountController(ApiService api) => _api = api;

        // Shows the login page
        [HttpGet] public IActionResult Login() => View();


        [HttpPost]
        public async Task<IActionResult> Login(LoginVm vm)
        // If the form inputs aren't valid, reload the page
        {
            if (!ModelState.IsValid) return View(vm);
            var (ok, error) = await  _api.LoginAsync(vm.Email, vm.Password);
            if (!ok) { ModelState.AddModelError("", error); return View(vm); }
            return RedirectToAction("Index", "Events");
        }

    }
