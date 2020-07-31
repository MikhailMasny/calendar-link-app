using Masny.WebApi.Contracts.Requests;
using Masny.WebApi.Helpers;
using Masny.WebApi.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Masny.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : BaseController
    {
        private readonly ICalendarService _calendarService;

        public CalendarController(ICalendarService calendarService)
        {
            _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<CalendarResponse>> GetCalendarLink(CalendarRequest calendarRequest)
        {
            var link = await _calendarService.GetGoogleCalendarLinkAsync(calendarRequest);

            return Ok(link);
        }
    }
}
