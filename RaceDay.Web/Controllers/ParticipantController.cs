using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{

    //User logged IN the session

    [SessionAuthorize("Participant")]
   public class ParticipantController : Controller
    {
      private readonly ApiService _api;

       public ParticipantController(ApiService api) => _api = api;

        // All events the Participant has enrolled for and their status
        [HttpGet]
        public async Task<IActionResult> MyEnrolments()
        {
            var res = await _api.GetListAsync<MyEnrollmentItem>("api/enrollments/mine"); //take users to their enrolments 

        if (!res.Ok) TempData["Error"] = res.Error;
            return View(res.Data ?? new List<MyEnrollmentItem>());
        }

        // The Participant's race history
        [HttpGet]
        public async Task<IActionResult> MyResults()
        {
          var res = await _api.GetListAsync<MyResultItem>("api/results/mine");

            if (!res.Ok) TempData["Error"] = res.Error;
            return View(res.Data ?? new List<MyResultItem>());
        }
    }
}
