using System;
using System.Collections.Generic;

namespace MuncipalityServiceRequestApp
{
    
    public class EventPriorityComparer : IComparer<(EventPriority Priority, DateTime Date)>
    {
        public int Compare((EventPriority Priority, DateTime Date) x, (EventPriority Priority, DateTime Date) y)
        {
            int priorityComparison = x.Priority.CompareTo(y.Priority);
            return priorityComparison != 0 ? priorityComparison : x.Date.CompareTo(y.Date);
        }
    }
}