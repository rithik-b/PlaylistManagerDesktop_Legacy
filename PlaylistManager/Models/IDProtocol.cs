using System.Threading;
using System.Threading.Tasks;

namespace PlaylistManager.Models
{
    public class IDProtocol : ILevelEncodedIDProtocol
    {
        public async Task<SearchResult?> Result(string input, CancellationToken? cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            
            var toReturn = true;
            
            await Task.Run(() =>
            {
                foreach (var c in input)
                {
                    if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                    {
                        toReturn = false;
                    }
                }
            }, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

            if (toReturn)
            {
                return new SearchResult(input, input.Length == 40 ? IDType.Hash : IDType.Key);
            }
            return null;
        }
    }
}