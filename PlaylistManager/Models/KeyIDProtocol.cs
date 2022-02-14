using System.Text.RegularExpressions;

namespace PlaylistManager.Models
{
    public class KeyIDProtocol : ILevelEncodedIDProtocol
    {
        private string kPattern = @"^([1234567890aAbBcCdDeEfF]+)$";
        
        public SearchResult? Result(string input)
        {
            if (Regex.IsMatch(input, kPattern))
            {
                return new SearchResult(input, IDType.Key);
            }
            return null;
        }
    }
}