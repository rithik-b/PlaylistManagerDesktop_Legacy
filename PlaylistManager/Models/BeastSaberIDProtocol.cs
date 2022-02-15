using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PlaylistManager.Models
{
    public class BeastSaberIDProtocol : ILevelEncodedIDProtocol
    {
        private string kPattern = @"^(https:\/\/)?(www.)?bsaber.com\/songs\/([a-fA-F0-9]+)$";
        
        public async Task<SearchResult?> Result(string input, CancellationToken? cancellationToken)
        {
            SearchResult? toReturn = null;
            
            await Task.Run(() =>
            {
                if (Regex.IsMatch(input, kPattern))
                {
                    var key = Regex.Replace(input, kPattern, "$3");
                    toReturn = new SearchResult(key, IDType.Key);
                }
            }, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
            
            return toReturn;
        }
    }
}