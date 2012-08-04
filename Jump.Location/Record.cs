using System;

namespace Jump.Location
{
    class Record
    {
        public Record(string fullName, decimal weight)
        {
            FullName = fullName;
            Weight = weight;
        }

        public string Provider { get; private set; }
        public string Path { get; private set; }

        public string FullName
        {
            get { return string.Format("{0}::{1}", Provider, Path); }
            set
            {
                var parts = value.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries);

                if (parts == null || parts.Length != 2)
                    throw new ArgumentException("Expected FullName in format of 'Provider::Path' but got " + value);

                Provider = parts[0];
                Path = parts[1];
            }
        }

        public decimal Weight { get; set; }
    }
}