namespace PlaylistManager.Models
{
    public class CustomLevel
    {
        private readonly string hash;
        
        /// <summary>
        /// Location of the song
        /// </summary>
        public readonly string Path;

        public CustomLevel(string hash, string path)
        {
            this.hash = hash;
            Path = path;
        }
    }
}