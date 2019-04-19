
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Podwoozka.Entities;
namespace Podwoozka.Models
{
    public class RaceItem
    {
        public long Id { get; set; }

        public float OriginLong { get; set; }
        public float OriginLat { get; set; }
        public float DestinationLong { get; set; }
        public float DestinationLat { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0: MM/dd/yy HH:mm}")]
        public DateTime StartTime { get; set; }

        public string[] Participants { get; set; }

        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd}")]
        public DateTime Schedule { get; set; }

        //public string[] Subs { get; set; }
        public float Cost { get; set; }
        public int Seats { get; set; }
        public int FreeSeats { get; set; }
        public string Owner { get; set; }
    }
}