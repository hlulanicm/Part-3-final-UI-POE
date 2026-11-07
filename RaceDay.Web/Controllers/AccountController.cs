using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;


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
            return RedirectToAction("Index", "Home");
        }




        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var res = await _api.RegisterAsync(vm);
            if (!res.Ok) { ModelState.AddModelError("", res.Error); return View(vm); }
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout() { await _api.LogoutAsync(); return RedirectToAction("Login"); }

        [HttpGet] public IActionResult Denied() => View();
    }



