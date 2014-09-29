using System;

namespace Jump.Location
{
    public class LocationNotFoundException : Exception
    {
        public LocationNotFoundException(string query)
            :base(string.Format("Could not find find a suitable match for search query '{0}' in database", query))
        {
        }
    }
}