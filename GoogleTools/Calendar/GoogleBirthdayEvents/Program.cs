using Google.Apis.Calendar.v3 ;
using Google.Apis.Calendar.v3.Data;
using GoogleAuth;
using GoogleEvents;

namespace GoogleBirthdayEvents
{
    internal class Program
    {

        const string CredentialsPath = "./credentials.json";
        const string TokenPath = "./token.json";
        const string ApplicationName = "Google Calendar API .NET Quickstart";
        
        private static string[] Scopes = {
        CalendarService.Scope.CalendarEvents,
        CalendarService.Scope.CalendarReadonly,
        CalendarService.Scope.Calendar
        };

        static void Main(string[] args)
        {

            var credentials = GoogleCredentialsBuilder.CreateFromFile(CredentialsPath, TokenPath, Scopes);
            var eventImporter = new GoogleCalendarImporter(credentials, ApplicationName);

            var calendars = eventImporter.GetCalendars();


            foreach ( var calendar in calendars )
            {
                var events = eventImporter.GetEventsFromCalendar(calendar.Id, new DateTime(2025, 1, 1), new DateTime(2025, 12, 31));
                
                foreach(var ev in events.Items)
                {
                    if (ev.EventType != "birthday")
                    {
                        Console.WriteLine($"Skipped event {ev.Summary}");
                        continue;
                    }

                    var newEvent = new Event()
                    {
                        Summary = ev.Summary,
                        Start = new EventDateTime
                        {
                            DateTime = ev.Start.DateTime,
                            TimeZone = ev.Start.TimeZone,
                            Date = ev.Start.Date
                        },
                        End = new EventDateTime
                        {
                            DateTime = ev.End.DateTime,
                            TimeZone = ev.End.TimeZone,
                            Date = ev.End.Date
                        },

                        Recurrence = ev.Recurrence,
                        RecurringEventId = ev.RecurringEventId,
                    };

                    newEvent.Reminders = new Event.RemindersData();
                    newEvent.Reminders.Overrides = [
                        new EventReminder() { 
                            Method = "popup", 
                            Minutes = 30
                        }];
                    newEvent.Reminders.UseDefault = false;

                    eventImporter.AddEvent(newEvent, calendar.Id);
                    Console.WriteLine($"Added alarm 30 minutes before event {ev.Summary}");
                }
            }

            Console.ReadLine();
        }
    }
}
