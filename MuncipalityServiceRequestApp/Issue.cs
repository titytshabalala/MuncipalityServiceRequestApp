using System;
using System.Collections.Generic;

namespace MuncipalityServiceRequestApp
{
    public class Issue
    {
        public string Location { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public List<string> AttachmentPaths { get; set; } = new List<string>();
        public DateTime DateReported { get; set; }

        // Ties into the "real-time status tracking" engagement strategy from the
        // research doc — this field is what the future Service Request Status
        // page will read from.
        public string Status { get; set; } = "Logged";
    }
}