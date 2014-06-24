using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterIt.Models
{
    public static class Shifts
    {
        public static readonly Shift Present = new Shift
        {
            Description = "The guard was present.",
            Id = Guid.Parse("0103948f-c244-43aa-8e03-200b130b91b3"),
            Symbol = "P",
            IsFixedDuration = true
        };

        public static readonly Shift Firearm = new Shift
        {
            Description = "The guard was present and had a firearm.",
            Id = Guid.Parse("15601fdf-a107-4255-a15a-5fcfec1413ff"),
            Symbol = "F",
            IsFixedDuration = true
        };

        public static readonly Shift Absent = new Shift
        {
            Description = "The guard was meant to be on duty but was absent.",
            Id = Guid.Parse("b5b958e7-61a3-4106-9972-644bc9689d5b"),
            Symbol = "A",
            IsFixedDuration = true
        };

        public static readonly Shift Leave = new Shift
        {
            Description = "The guard was on leave.",
            Id = Guid.Parse("daacb7a4-1beb-4c73-ad85-e427aef2d62f"),
            Symbol = "L",
            IsFixedDuration = true
        };

        public static readonly Shift Off = new Shift
        {
            Description = "The guard was on leave.",
            Id = Guid.Parse("14550ed1-b36a-45ec-aa6c-acb324ad65a5"),
            Symbol = "O",
            IsFixedDuration = true
        };

        public static readonly Shift Sick = new Shift
        {
            Description = "The guard was sick.",
            Id = Guid.Parse("e65572d6-9956-4a1b-bb91-1e91d2820061"),
            Symbol = "S",
            IsFixedDuration = true
        };


        public static readonly Shift Overtime = new Shift
        {
            Description = "The guard worked overtime.",
            Id = Guid.Parse("44208e8f-915e-4e1f-b4e1-b7f11faa61b2"),
            Symbol = "+",
            IsFixedDuration = false,
            AvailableDurations = new int[] { 1,2,3,4,5,6,7,8,9,10,11,12 }
        };

        public static readonly Shift ExtraShift = new Shift
        {
            Description = "The guard worked an extra day.",
            Id = Guid.Parse("a681fa46-3ef7-4d6d-a430-3c59a72380af"),
            Symbol = "X",
            IsFixedDuration = true
        };

        public static readonly IReadOnlyCollection<Shift> All = (new List<Shift> { Present, Firearm, Absent, Leave, Off, Sick, ExtraShift, Overtime }).AsReadOnly();

    }
}
