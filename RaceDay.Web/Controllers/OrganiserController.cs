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

//GET EVENT TO EDIT THE EVENT
        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            var res = await _api.GetAsync<EventItem>($"api/events/{id}");

         if (!res.Ok || res.Data == null)
            {
              TempData["Error"]  = res.Error;
                return RedirectToAction("Events");
            }

            // Organisers can only edit their own events
          if (res.Data.OrganiserID != _api.UserId)
            {
                TempData["Error"] = "You can only edit events that you created.";//if the user tries to edit someone elses event display an error message.
                return RedirectToAction("Events");
            }

            var e = res.Data;

            //Create the event form
            return View("EventForm", new EventVm
            {
                //form elements

                EventID = e.EventID, //event ID
                Name = e.Name,//name
                Description = e.Description,//description
                EventDate = e.EventDate,//date
                Location = e.Location,//location
                Distance = e.Distance,//distance
                EventType = e.EventType,//type
                TargetAudience = e.TargetAudience,//target audience
                BannerUrl = e.BannerUrl//url

            });
        }
        //POST TO UPDATE THE EVENT

        [HttpPost]

        public async Task<IActionResult> EditEvent(int id, EventVm vm)
        {
         vm.EventID = id;
           ValidateBanner(vm);

            if (!ModelState.IsValid) return View("EventForm", vm);

          var updated = await _api.PutAsync($"api/events/{id}", ToBody(vm));

            if (!updated.Ok)

            {
                //if the post was unsuccsesfull show an appropriate error.
            ModelState.AddModelError("", updated.Error);

                return View("EventForm", vm);
            }

            if (vm.Banner != null && vm.Banner.Length > 0)//if no banner is found
            {
                var upload = await _api.PostFileAsync($"api/events/{id}/banner", vm.Banner);
                if (!upload.Ok)
                {
                    TempData["Warning"] = "The event was updated, but the banner was not uploaded: " + upload.Error; //display an appropriate error
                    return RedirectToAction("Events");
                }
            }

            TempData["Success"] = "The event has been updated.";
            return RedirectToAction("Events");
        }

        //Delete event
        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var res = await _api.DeleteAsync($"api/events/{id}");

         if (res.Ok) TempData["Success"] = "The event has been deleted."; //show an appropriate delete message
            else TempData["Error"] = res.Error;//else display an appropriate error message

            return RedirectToAction("Events");//redirect to the events page
        }