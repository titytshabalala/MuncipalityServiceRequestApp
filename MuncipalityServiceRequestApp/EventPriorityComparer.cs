using System.Collections.Generic;

namespace MuncipalityServiceRequestApp
{
    /// <summary>Orders events most urgent first, then soonest date first.</summary>
    public class EventPriorityComparer : IComparer<MunicipalEvent>
    {
        public int Compare(MunicipalEvent x, MunicipalEvent y)
        {
            int priority = x.Priority.CompareTo(y.Priority);
            return priority != 0 ? priority : x.Date.CompareTo(y.Date);
        }
    }
}