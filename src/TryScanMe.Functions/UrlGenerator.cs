using System;
using System.Collections.Generic;

namespace TryScanMe.Functions
{
    public class UrlGenerator
    {
        public List<Guid> Guids { get; set; } = new List<Guid>();
        public Uri Uri { get; private set; }

        public UrlGenerator(Uri uri, int number)
        {
            Uri = uri;

            for (var i = 0; i < number; i++)
                Guids.Add(Guid.NewGuid());
        }

        public UrlGenerator(Uri uri, int number, Guid guidToCopy)
        {
            Uri = uri;

            for (var i = 0; i < number; i++)
                Guids.Add(guidToCopy);
        }

        public List<Uri> ToUri()
        {
            List<Uri> uris = new List<Uri>();

            foreach (Guid g in Guids)
            {
                uris.Add(new Uri(Uri + g.ToString("N")));
            }

            return uris;
        }
    }
}
