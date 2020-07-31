using System;
using System.ComponentModel.DataAnnotations;

namespace Masny.WebApi.Contracts.Requests
{
    public class CalendarRequest
    {
        public string Text { get; set; }

        [Required]
        public DateTime Start { get; set; }

        [Required]
        public DateTime End { get; set; }

        public string Details { get; set; }

        public string Location { get; set; }
    }
}
