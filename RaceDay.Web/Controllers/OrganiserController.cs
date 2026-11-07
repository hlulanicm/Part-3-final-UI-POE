using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Filters;
using RaceDay.Web.Services;
using RaceDay.Web.ViewModels;
using System.Globalization;

namespace RaceDay.Web.Controllers
{
    //Allow only when the organiser is logged in.
    [SessionAuthorize("Organiser")]
    public class OrganiserController : Controller
    {

        // Declares a private, read-only field named '_api' of type 'ApiService'
        private readonly ApiService _api;


    public OrganiserController(ApiService api) => _api = api;

        // Organiser Dashboard 

        // Summary of the Organiser's events, enrolments and upcoming dates
        //Get HTTP request will be used to display all the info
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
        var res = await _api.GetListAsync<OrganiserEventItem>("api/events/mine");//Takes the organiser to their dashboard

            if (!res.Ok) TempData["Error"] = res.Error;

            return View(new DashboardVm { Events = res.Data ?? new() });
        }


        // Events 

      [HttpGet]
     public async Task<IActionResult> Events() //Get all the events for the organiser 

        {
            var res = await _api.GetListAsync<OrganiserEventItem>("api/events/mine");//redirect them to their events

        if (!res.Ok) TempData["Error"] = res.Error;
            return View(res.Data ?? new List<OrganiserEventItem>());
        }

        [HttpGet]
     public IActionResult CreateEvent() => View("EventForm", new EventVm { EventDate = DateTime.Today.AddDays(30) });
        //Post changes 
        [HttpPost]

       //CREATE EVENT
    //The bannner will be uploaded to Azure Blob Storage 
     public async Task<IActionResult> CreateEvent(EventVm vm)
        {

        ValidateBanner(vm);

            if (!ModelState.IsValid) return View("EventForm", vm);


        var created = await _api.PostAsync<EventItem>("api/events", ToBody(vm));
          if (!created.Ok || created.Data == null)

            {
                ModelState.AddModelError("", created.Error);//upon a creation error view the event form 
                return View("EventForm", vm);
            }

            // The banner goes through the API, which stores it in Azure Blob Storage
          if (vm.Banner != null && vm.Banner.Length > 0)
            {
            var upload = await _api.PostFileAsync($"api/events/{created.Data.EventID}/banner", vm.Banner);
                if (!upload.Ok)//Upload event Banner 
                {

               TempData["Warning"] = "The event was created, but the banner was not uploaded: " + upload.Error;//show appropriate error upon unsucessful upload
                    return RedirectToAction("Events");
                }
            }


            TempData["Success"] = "The event has been created. Add categories so Participants can enrol.";//Show an appropriate error upon sucessful participant enrolment
            return RedirectToAction("Categories", new { eventId = created.Data.EventID });
        }


