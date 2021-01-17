using Microsoft.WindowsAzure.Storage.Table;

namespace TryScanMe.Functions.Entities
{
    public class Walls : TableEntity
    {
        public Walls(string guid, double latitude, double longitude, string title, int maxDistance)
        {
            PartitionKey = guid;
            RowKey = guid;
            Latitude = latitude;
            Longitude = longitude;
            MaxDistance = maxDistance;
            Title = string.IsNullOrWhiteSpace(title) ? Constants.Default.Wall.Title : title;
        }

        public Walls(string guid, double latitude, double longitude, string title)
        {
            new Walls(guid, latitude, longitude, title, maxDistance: MAX_DISTANCE_ALLOWED);
        }

        public Walls() { }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool Deleted { get; set; } = false;
        public string Title { get; set; } = Constants.Default.Wall.Title;
        private int _maxDistance;
        public const int MAX_DISTANCE_ALLOWED = 100; //meters

        public int MaxDistance
        {
            get
            {
                return _maxDistance == 0 ? MAX_DISTANCE_ALLOWED : _maxDistance;
            }
            set
            {
                _maxDistance = value;
            }
        }
    }
}

