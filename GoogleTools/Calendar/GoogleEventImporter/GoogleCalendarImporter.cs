using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System.Runtime.CompilerServices;

namespace GoogleEvents
{
    public class GoogleCalendarImporter
    {
        //private string[] Scopes = {
        //CalendarService.Scope.CalendarEvents,
        //CalendarService.Scope.CalendarReadonly,
        //CalendarService.Scope.Calendar
        //};

        //private readonly GoogleOptions options;
        private CalendarService calendarService;
        private string applicationName;

        public GoogleCalendarImporter(UserCredential credentials, string applicationName)
        {
            this.applicationName = applicationName;
            Events = new List<EventDay>();
            Calendars = new List<CalendarListEntry>();

            //var credentials = GoogleCredentialsBuilder.CreateFromFile(options.CredentialsPath, options.TokenPath, );

            if (credentials == null)
            {
                throw new UnauthorizedAccessException("Could not authenticate to Google Calendar");
            }

            calendarService = GetCalendarService(credentials);
        }

        public IList<EventDay> Events { get; private set; }
        public IList<CalendarListEntry> Calendars { get; private set; }
        public Dictionary<int, int> CommittedHolidayDaysPerYear = new Dictionary<int, int>();

        //public void RefreshEvents(DateTime startTime, DateTime endTime)
        //{
        //    CommittedHolidayDaysPerYear.Clear();
        //    Calendars.Clear();
        //    Events.Clear();

        //    Calendars = GetCalendars();
        //    Events = GetEvents(startTime, endTime);
        //}

        /// <summary>
        /// Add event to calndar.
        /// </summary>
        /// <param name="event"></param>
        public void AddEvent(EventDay @event)
        {
            var googleEvent = new Event()
            {
                Start = new EventDateTime
                {
                    Date = @event.StartDate.ToString("yyyy-MM-dd")
                },
                End = new EventDateTime
                {
                    Date = @event.EndDate.AddMinutes(1).ToString("yyyy-MM-dd")
                },
                Summary = @event.Description,
                Description = @event.Description,
            };

            calendarService.Events.Insert(googleEvent, @event.CalendarId).Execute();
        }

        /// <summary>
        /// Add event to calndar.
        /// </summary>
        /// <param name="event"></param>
        public void AddEvent(Event @event, string calendarId)
        {
            calendarService.Events.Insert(@event, calendarId).Execute();
        }

        /// <summary>
        /// Delete event from calendar.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="calendarId"></param>
        public void DeleteEvent(string eventId, string calendarId)
        {
            calendarService.Events.Delete(calendarId, eventId).Execute();
        }

        /// <summary>
        /// Update event in calendar.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="eventId"></param>
        /// <param name="calendarId"></param>
        public void EditEvent(EventDay @event, string eventId, string calendarId)
        {
            var googleEvent = new Event()
            {
                Start = new EventDateTime
                {
                    Date = @event.StartDate.ToString("yyyy-MM-dd")
                },
                End = new EventDateTime
                {
                    Date = @event.EndDate.AddMinutes(1).ToString("yyyy-MM-dd")
                },
                Summary = @event.Description,
                Description = @event.Description,
            };

            calendarService.Events.Update(googleEvent, calendarId, eventId).Execute();
        }

        /// <summary>
        /// Update event in calendar.
        /// </summary>
        /// <param name="event"></param>
        /// <param name="eventId"></param>
        /// <param name="calendarId"></param>
        public void AddAlarmToEvent(Event @event, string eventId, string calendarId, int minutesBeforeEvent)
        {
            @event.Reminders.Overrides.Add(new EventReminder() { Method = "popup", Minutes = minutesBeforeEvent });
            calendarService.Events.Update(@event, calendarId, eventId).Execute();
        }

        /// <summary>
        /// Get event names for a single day.
        /// </summary>
        /// <param name="selectedDate"></param>
        /// <returns></returns>
        public IEnumerable<string> GetEventNamesForDay(DateTime selectedDate) =>
            Events.Where(ev =>
            {
                return (ev.StartDate <= selectedDate.Date && selectedDate.Date <= ev.EndDate) ? true : false;
            }).Select(ev => ev.Description);

        /// <summary>
        /// Get events for a day.
        /// </summary>
        /// <param name="selectedDate"></param>
        /// <returns></returns>
        public IEnumerable<EventDay> GetEventsForDay(DateTime selectedDate) =>
        Events.Where(ev =>
        {
            return (ev.StartDate <= selectedDate.Date && selectedDate.Date < ev.EndDate) ? true : false;
        });

        /// <summary>
        /// Get events from Google within a start adn end date.
        /// </summary>
        /// <param name="calendarName"></param>
        /// <param name="service"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public Events GetEventsFromCalendar(string calendarName, DateTime startTime, DateTime endTime)
        {
            EventsResource.ListRequest request;

            request = calendarService.Events.List(calendarName);
            request.TimeMin = startTime;
            request.TimeMax = endTime;
            request.SingleEvents = true;
            request.MaxResults = 10000;
            return request.Execute();
        }

        /// <summary>
        /// Gets calendars from Google.
        /// </summary>
        /// <param name="calendarService"></param>
        public IList<CalendarListEntry> GetCalendars()
        {
            return calendarService.CalendarList.List().Execute().Items;
        }

        /// <summary>
        /// Gets the calendar service.
        /// </summary>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private CalendarService GetCalendarService(UserCredential credentials)
        {
            if (credentials == null)
                return null;

            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = applicationName,
            });

            return service;
        }

        /// <summary>
        /// Get all events from Google from all calendars.
        /// </summary>
        /// 
        public IList<EventDay> GetEvents(CalendarListEntry calendar, DateTime startTime, DateTime endTime)
        {
            var events = new List<EventDay>();
            var eventsInCalendar = GetEventsFromCalendar(calendar.Id, startTime, endTime).Items;

            foreach (var ev in eventsInCalendar)
            {
                try
                {
                    var eventDay = new EventDay()
                    {
                        Id = ev.Id,
                        Calendar = calendar.Summary,
                        CalendarBkColor = calendar.BackgroundColor,
                        CalendarId = calendar.Id,
                        Description = ev.Summary,
                        StartDate = DateTime.Parse(ev.Start.Date),
                        EndDate = DateTime.Parse(ev.End.Date),
                    };

                    events.Add(eventDay);

                    //if (vacationWords.Any(s=> eventDay.Description.IndexOf(s,StringComparison.InvariantCultureIgnoreCase) !=-1))
                    //{
                    //    if (CommittedHolidayDaysPerYear.ContainsKey(eventDay.StartDate.Year))
                    //    {
                    //        CommittedHolidayDaysPerYear[eventDay.StartDate.Year]+=(eventDay.EndDate - eventDay.StartDate).Days;
                    //    }
                    //    else
                    //    {
                    //        CommittedHolidayDaysPerYear[eventDay.StartDate.Year] = (eventDay.EndDate - eventDay.StartDate).Days;
                    //    }
                    //}
                }
                catch (Exception)
                {
                    Console.WriteLine($"Id:{ev.Id}");
                    Console.WriteLine($"StartDate:{ev.Start.Date}");
                    Console.WriteLine($"EndDate:{ev.End.Date}");
                    Console.WriteLine($"Description:{ev.Summary}");
                }
            }

            return events;
        }

        public Event CreateEventCopy(Event sourceEvent)
        {
            var newEvent = new Event
            {
                Summary = sourceEvent.Summary,
                Location = sourceEvent.Location,
                Description = sourceEvent.Description,
                Start = new EventDateTime
                {
                    DateTime = sourceEvent.Start.DateTime,
                    TimeZone = sourceEvent.Start.TimeZone,
                    Date = sourceEvent.Start.Date
                },
                End = new EventDateTime
                {
                    DateTime = sourceEvent.End.DateTime,
                    TimeZone = sourceEvent.End.TimeZone,
                    Date = sourceEvent.End.Date
                },
                RecurringEventId = sourceEvent.RecurringEventId,
                OriginalStartTime = sourceEvent.OriginalStartTime != null ? new EventDateTime
                {
                    DateTime = sourceEvent.OriginalStartTime.DateTime,
                    TimeZone = sourceEvent.OriginalStartTime.TimeZone,
                    Date = sourceEvent.OriginalStartTime.Date
                } : null,
                Transparency = sourceEvent.Transparency,
                Visibility = sourceEvent.Visibility,
                ICalUID = sourceEvent.ICalUID,
                Sequence = sourceEvent.Sequence,
                Attendees = sourceEvent.Attendees?.Select(a => new EventAttendee
                {
                    Email = a.Email,
                    DisplayName = a.DisplayName,
                    ResponseStatus = a.ResponseStatus,
                    Optional = a.Optional,
                    Comment = a.Comment,
                    AdditionalGuests = a.AdditionalGuests
                }).ToList(),
                ExtendedProperties = sourceEvent.ExtendedProperties != null ? new Event.ExtendedPropertiesData
                {
                    Private__ = sourceEvent.ExtendedProperties.Private__?.ToDictionary(p => p.Key, p => p.Value),
                    Shared = sourceEvent.ExtendedProperties.Shared?.ToDictionary(p => p.Key, p => p.Value)
                } : null,
                Reminders = sourceEvent.Reminders != null ? new Event.RemindersData
                {
                    UseDefault = sourceEvent.Reminders.UseDefault,
                    Overrides = sourceEvent.Reminders.Overrides?.Select(r => new EventReminder
                    {
                        Method = r.Method,
                        Minutes = r.Minutes
                    }).ToList()
                } : null,
                Source = sourceEvent.Source != null ? new Event.SourceData
                {
                    Title = sourceEvent.Source.Title,
                    Url = sourceEvent.Source.Url
                } : null,
                Attachments = sourceEvent.Attachments?.Select(a => new EventAttachment
                {
                    FileUrl = a.FileUrl,
                    Title = a.Title,
                    MimeType = a.MimeType,
                    IconLink = a.IconLink,
                    FileId = a.FileId
                }).ToList(),
                GuestsCanInviteOthers = sourceEvent.GuestsCanInviteOthers,
                GuestsCanModify = sourceEvent.GuestsCanModify,
                GuestsCanSeeOtherGuests = sourceEvent.GuestsCanSeeOtherGuests,
                PrivateCopy = sourceEvent.PrivateCopy,
                Locked = sourceEvent.Locked,
                ColorId = sourceEvent.ColorId
            };

            return newEvent;
        }

        internal void ChageSelectStatus(string text, bool isChecked)
        {
            var calendarToChange = calendarService.CalendarList.List().Execute().Items.First(x => x.Summary.Equals(text));

            if (calendarToChange != null)
            {

                calendarToChange.Selected = isChecked;
                calendarService.CalendarList.Update(calendarToChange, calendarToChange.Id).Execute();
            }
        }
    }
}

