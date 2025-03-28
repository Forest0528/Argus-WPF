using System;
using System.Text.Json.Serialization;

namespace Argus_Project
{
    public class TimeRecord
    {
        public string EmployeeName { get; set; }
        public DateTime ArrivalTime { get; set; }
        public DateTime? DepartureTime { get; set; }

        [JsonIgnore]
        public double HoursWorked => DepartureTime.HasValue ? (DepartureTime.Value - ArrivalTime).TotalHours : 0;

        [JsonIgnore]
        public string HoursWorkedDisplay => DepartureTime.HasValue ?
            $"{(int)(DepartureTime.Value - ArrivalTime).TotalHours}ч {(DepartureTime.Value - ArrivalTime).Minutes}м" : "-";
    }
}