using System;

namespace Jump.Location
{
    public class LocationNotFoundException : Exception
    {
        public LocationNotFoundException(string directory)
            :base(string.Format("Could not find find a suitable match for search term {0} in database", directory))
        {
        }
    }
}