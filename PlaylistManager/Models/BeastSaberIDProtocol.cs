using System.Text.RegularExpressions;

namespace PlaylistManager.Models
{
    public class BeastSaberIDProtocol : ILevelEncodedIDProtocol
    {
        private string kPattern = @"^(https:\/\/)?(www.)?bsaber.com\/songs\/([1234567890aAbBcCdDeEfF]+)$";
        
        public SearchResult? Result(string input)
        {
            if (Regex.IsMatch(input, kPattern))
            {
                var key = Regex.Replace(input, kPattern, "$3");
                return new SearchResult(key, IDType.Key);
            }
            return null;
        }
    }
}