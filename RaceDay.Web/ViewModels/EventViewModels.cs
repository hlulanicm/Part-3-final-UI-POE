using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class EventItem
    {
        public int EventID { get; set; }
        public int OrganiserID { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public decimal? Distance { get; set; }
        public string EventType { get; set; } = "";
        public string? TargetAudience { get; set; }
        public string? BannerUrl { get; set; }
    }

    public class CategoryItem
    {
        public int CategoryID { get; set; }
        public int EventID { get; set; }
        public string CategoryType { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal? Distance { get; set; }
        public int MaxParticipants { get; set; }
    }

    public class OrganiserEventItem
    {
        public int EventID { get; set; }
        public string Name { get; set; } = "";
        public string EventType { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public int CategoryCount { get; set; }
        public int EnrollmentCount { get; set; }
    }

    public class EventEnrollmentItem
    {
        public int EnrollmentID { get; set; }
        public string ParticipantName { get; set; } = "";
        public string Email { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = "";
        public int? Position { get; set; }
        public TimeSpan? FinishTime { get; set; }
    }

    public class MyEnrollmentItem
    {
        public int EnrollmentID { get; set; }
        public int EventID { get; set; }
        public int CategoryID { get; set; }
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = "";
    }

    public class MyResultItem
    {
        public string EventName { get; set; } = "";
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = "";
        public TimeSpan? FinishTime { get; set; }
        public int? Position { get; set; }
        public int TotalFinishers { get; set; }
    }

    public class HomeVm
    {
        public List<EventItem> Events { get; set; } = new();
        public string? Search { get; set; }
        public string? EventType { get; set; }
        public string? Location { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public IEnumerable<string> EventTypes { get; set; } = Helpers.Lookups.EventTypes;
    }

    public class EventDetailsVm
    {
        public EventItem Event { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();

        public Dictionary<int, string> MyStatusByCategory { get; set; } = new();
    }

    public class DashboardVm
    {
        public List<OrganiserEventItem> Events { get; set; } = new();

        public int TotalEnrolments => Events.Sum(e => e.EnrollmentCount);

        public List<OrganiserEventItem> Upcoming =>
            Events.Where(e => e.EventDate.Date >= DateTime.Today).OrderBy(e => e.EventDate).ToList();
    }

    public class EventEnrolmentsVm
    {
        public EventItem Event { get; set; } = new();
        public List<EventEnrollmentItem> Enrolments { get; set; } = new();
    }

    public class CategoriesVm
    {
        public EventItem Event { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();
        public CategoryVm Form { get; set; } = new();
    }

    public class EventVm
    {
        public int EventID { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Event name")]
        public string Name { get; set; } = "";

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please choose the event date.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime? EventDate { get; set; }

        [Required, StringLength(200)]
        public string Location { get; set; } = "";

        [Range(0.1, 1000)]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Required]
        [Display(Name = "Event type")]
        public string EventType { get; set; } = "Road Run";

        [StringLength(50)]
        [Display(Name = "Target audience")]
        public string? TargetAudience { get; set; }

        public string? BannerUrl { get; set; }

        [Display(Name = "Banner image")]
        public IFormFile? Banner { get; set; }
    }

    public class CategoryVm
    {
        [Required]
        [Display(Name = "Category type")]
        public string CategoryType { get; set; } = "Distance";

        [Required, StringLength(50)]
        public string Name { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.1, 1000)]
        [Display(Name = "Distance (km)")]
        public decimal? Distance { get; set; }

        [Range(1, 100000)]
        [Display(Name = "Places available")]
        public int MaxParticipants { get; set; } = 100;
    }
}
