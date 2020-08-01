using Masny.WebApi.Contracts.Requests;
using System.Threading.Tasks;

namespace Masny.WebApi.Interfaces
{
    public interface ICalendarService
    {
        Task<CalendarResponse> GetGoogleCalendarLinkAsync(CalendarRequest calendarRequest);
    }
}
