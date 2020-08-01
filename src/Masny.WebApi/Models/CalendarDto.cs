using System;

namespace Masny.WebApi.Models
{
    public class CalendarDto
    {
        public string Text { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string Details { get; set; }

        public string Location { get; set; }
    }
}
