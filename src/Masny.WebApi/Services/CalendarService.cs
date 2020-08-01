using Masny.WebApi.Extensions;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Masny.WebApi.Models;
using System.Threading.Tasks;

namespace Masny.WebApi.Services
{
    public class CalendarService : ICalendarService
    {
        public Task<string> GetGoogleCalendarLinkAsync(CalendarDto calendarDto)
        {
            var link = $"{Constants.GoogleUrl}" +
                $"?action=TEMPLATE&text={calendarDto.Text}" +
                $"&dates={calendarDto.Start.ToLocalString()}" +
                $"/{calendarDto.End.ToLocalString()}" +
                $"&details={calendarDto.Details}" +
                $"&location={calendarDto.Location}";

            return Task.FromResult(link);
        }
    }
}
