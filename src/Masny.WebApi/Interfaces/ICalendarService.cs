using Masny.WebApi.Contracts.Requests;
using System.Threading.Tasks;

namespace Masny.WebApi.Interfaces
{
    public interface ICalendarService
    {
        Task<string> GetGoogleCalendarLinkAsync(CalendarRequest calendarRequest);
    }
}
