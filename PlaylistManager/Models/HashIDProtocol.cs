using System.Text.RegularExpressions;

namespace PlaylistManager.Models
{
    public class HashIDProtocol : ILevelEncodedIDProtocol
    {
        private string kPattern = "^[a-f0-9]{64}$";
        
        public SearchResult? Result(string input)
        {
            if (Regex.IsMatch(input, kPattern))
            {
                return new SearchResult(input, IDType.Hash);
            }
            return null;
        }
    }
}