namespace TryScanMe.Functions
{
    public static class Constants
    {
        public static class Default
        {
            public static class Wall
            {
                public static string Title = "Untitled Wall";
                public static string WelcomeMessage = "Welcome to Potty Mouth - Leave a message on the wall";
                public static string User = "PottyMouth";
            }
        }
        public static class BlobContainerNames
        {
            public static string Wall = "walls";
            public static string Image = "images";
        }
        public static class TableNames
        {
            public static string Tracked = "Tracked";
            public static string TemporaryUrl = "TemporaryUrl";
            public static string Users = "Users";
            public static string Walls = "Walls";
        }
    }
}
