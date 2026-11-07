using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;

namespace RaceDay.Web.Controllers
{
    [SessionAuthorize]
  public class EventsController : Controller
    {
        private readonly ApiService _api;

          public EventsController(ApiService api) => _api = api;

        //  GET Full event information and its categories
      [HttpGet]

        public async Task<IActionResult> Details(int id)
        {
            var ev = await _api.GetAsync<EventItem>($"api/events/{id}");

              if (!ev.Ok || ev.Data ==  null)
            {
              TempData["Error"] = ev.Error;

                return RedirectToAction("Index", "Home");

           }



       var cats =  await _api.GetListAsync<CategoryItem> ($"api/events/{id}/categories");

       var vm  = new EventDetailsVm { Event = ev.Data,  Categories = cats.Data ?? new() };
            if (!cats.Ok) TempData["Error"] = cats.Error;

            // A Participant sees which categories they have already enrolled in

         if (_api.IsParticipant)
            {
                var mine = await _api.GetListAsync<MyEnrollmentItem>("api/enrollments/mine");
               if (mine.Ok)

                    vm.MyStatusByCategory = mine.Data!.ToDictionary(e => e.CategoryID, e => e.Status);
            }

            return View(vm);
        }

       

        // A logged-in Participant enrols in a category
        //Post requests will be done 
        [HttpPost]
        [SessionAuthorize("Participant")]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            // Sends an asynchronous HTTP POST request to the "api/enrollments" endpoint
            // and stores the response in the res variable.

            var res = await _api.PostAsync("api/enrollments", new { categoryID = categoryId });


            if (res.Ok) TempData["Success"] = "You have entered this event. Your entry is pending until the Organiser confirms it.";//Display appropriate message upon sucessful registration
            else TempData["Error"] = res.Error;

         return RedirectToAction("Details", new { id = eventId });
        }


    }
}
