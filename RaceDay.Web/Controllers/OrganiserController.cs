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