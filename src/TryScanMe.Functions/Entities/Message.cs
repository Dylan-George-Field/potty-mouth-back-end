using System;
using TryScanMe.Functions.Extensions;

namespace TryScanMe.Functions.Entities
{
    public class Message
    {
        public DateTime Timestamp { get; set; }
        public string Text { get; set; }
        public string ImageUri { get; set; }
        public string Filename { get; set; }

        private string _nameId;
        public string NameId
        {
            get => _nameId;
            set => _nameId = string.IsNullOrEmpty(value) ? null : value;
        }

        private string friendlyTime;
        public string FriendlyTime {
            get => friendlyTime = Timestamp.ToFriendlyDate();
            set => friendlyTime = value;
        }

        private string _username { get; set; }
        public string Username
        {
            get => _username;
            set => _username = string.IsNullOrEmpty(value) ? "Anon" : value;
        }

        public Message()
        {
            Timestamp = DateTime.UtcNow;
            FriendlyTime = "Just now";
        }
    }
}
