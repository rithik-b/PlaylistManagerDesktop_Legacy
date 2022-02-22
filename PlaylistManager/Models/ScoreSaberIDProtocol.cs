using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlaylistManager.Utilities;
using Splat;

namespace PlaylistManager.Models
{
    public class ScoreSaberIDProtocol : ILevelEncodedIDProtocol
    {
        private const string kPattern = @"^(https:\/\/)?(www.)?scoresaber.com\/leaderboard\/([0-9]+)$";
        
        private HttpClientService? httpClientService;
        private HttpClientService HttpClientService =>
            httpClientService ??= Locator.Current.GetService<HttpClientService>()!;
        
        public async Task<SearchResult?> FindResultAsync(string input, CancellationToken? cancellationToken = null)
        {
            string? leaderboardID = null;
            
            await Task.Run(() =>
            {
                if (Regex.IsMatch(input, kPattern))
                {
                    leaderboardID = Regex.Replace(input, kPattern, "$3");
                }
            }, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);

            if (leaderboardID != null)
            {
                var webResponse = await HttpClientService.GetAsync(
                    $"https://scoresaber.com/api/leaderboard/by-id/{leaderboardID}/info",
                    cancellationToken ?? CancellationToken.None, null);
                
                using (StreamReader streamReader = new StreamReader(await webResponse.ReadAsStreamAsync()))
                {
                    using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                    {
                        JsonSerializer jsonSerializer = new JsonSerializer();
                        var response = jsonSerializer.Deserialize<ScoreSaberWebResponse>(jsonTextReader);
                        if (response is {songHash: { }})
                        {
                            return new SearchResult(response.songHash, IDType.Hash);
                        }
                    }
                }
            }

            return null;
        }
    }

    public struct ScoreSaberWebResponse
    {
        [JsonProperty]
        public string? songHash { get; set; }
    }
}