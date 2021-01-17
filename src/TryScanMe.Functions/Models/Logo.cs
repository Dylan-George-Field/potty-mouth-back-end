using System.Drawing;

namespace TryScanMe.Functions.Models
{
    public class Logo
    {
        public Image Image { get; }

        private const string filepath = "";

        public Logo(string location)
        {
            Image = Image.FromFile(location);
        }

        public Logo() => new Logo(filepath);
    }
}
