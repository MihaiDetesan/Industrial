using System;

namespace GoogleEvents
{
    public class EventDay
    {
        public string Id { get; set; }
        public string Calendar { get; set; }
        public string CalendarBkColor { get; set; }
        public string CalendarId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
    }
}
