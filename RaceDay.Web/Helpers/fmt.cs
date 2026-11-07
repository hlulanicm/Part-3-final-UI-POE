namespace RaceDay.Web.Helpers
{
    // Display helpers used by the views
    public static class Fmt
    {
        // Shows a finish time as hh:mm:ss
        public static string Time(TimeSpan? t) =>
            t == null ? "-" : $"{(int)t.Value.TotalHours:00}:{t.Value.Minutes:00}:{t.Value.Seconds:00}";

        // Shows a position of the user ( as 1st, 2nd, 3rd, 4th ...)
        public static string Ordinal(int n)
        {

            //display a suffix to the end of the position 
            string suffix;
            if (n % 100 >= 11 && n % 100 <= 13) suffix = "th";
            else if (n % 10 == 1) suffix = "st";
            else if (n % 10 == 2) suffix = "nd";
            else if (n % 10 == 3) suffix = "rd";
            else suffix = "th";
            return n + suffix;
        }
        //store the event in a date and time string
        public static string Date(DateTime d) => d.ToString("dd MMM yyyy");
    }

    // Fixed lists used by the forms
    public static class Lookups
    {
        public static readonly string[] EventTypes = { "Road Run", "Trail Run", "Cycling", "Walk", "Triathlon", "Other" };//event names 
        public static readonly string[] CategoryTypes = { "Distance", "Age" }; //category types
        public static readonly string[] Statuses = { "Pending", "Confirmed", "Cancelled" };//event state
    }
}
