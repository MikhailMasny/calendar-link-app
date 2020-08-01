using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Extensions;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using System.Threading.Tasks;

namespace Masny.WebApi.Services
{
    public class CalendarService : ICalendarService
    {
        public Task<CalendarResponse> GetGoogleCalendarLinkAsync(CalendarRequest calendarRequest)
        {
            var response = new CalendarResponse
            {
                Link = $"{Constants.GoogleUrl}" +
                    $"?action=TEMPLATE&text={calendarRequest.Text}" +
                    $"&dates={calendarRequest.Start.ToLocalString()}" +
                    $"/{calendarRequest.End.ToLocalString()}" +
                    $"&details={calendarRequest.Details}" +
                    $"&location={calendarRequest.Location}"
            };

            return Task.FromResult(response);
        }
    }
}
