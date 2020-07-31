using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using System.Threading.Tasks;

namespace Masny.WebApi.Services
{
    public class CalendarService : ICalendarService
    {
        public Task<string> GetGoogleCalendarLinkAsync(CalendarRequest calendarRequest)
        {
            var date1 = calendarRequest.Start.AddHours(-3).ToString("yyyyMMddTHHmmssZ");
            var date2 = calendarRequest.End.AddHours(-3).ToString("yyyyMMddTHHmmssZ");
            var link = $"{Constants.GoogleUrl}?action=TEMPLATE&text={calendarRequest.Text}&dates={date1}/{date2}&details={calendarRequest.Details}&location={calendarRequest.Location}";

            return Task.FromResult(link);
        }
    }
}
