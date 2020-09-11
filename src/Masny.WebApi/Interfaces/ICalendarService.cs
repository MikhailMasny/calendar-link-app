using Masny.WebApi.Models;
using System.Threading.Tasks;

namespace Masny.WebApi.Interfaces
{
    public interface ICalendarService
    {
        Task<string> GetGoogleCalendarLinkAsync(CalendarDto calendarDto);
    }
}
