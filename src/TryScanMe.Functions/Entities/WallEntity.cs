using System.Collections.Generic;

namespace TryScanMe.Functions.Entities
{
    public class WallEntity
    {
        public List<Message> Messages = new List<Message>();

        public WallEntity(Message message)
        {
            Messages.Add(message);
        }
    }
}
