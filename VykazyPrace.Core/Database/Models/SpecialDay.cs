using System;

namespace VykazyPrace.Core.Database.Models
{
    public partial class SpecialDay
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        public string Title { get; set; } = "Default";

        public bool Locked { get; set; }

        public string Color { get; set; } = "#FFCDC7";
    }
}
