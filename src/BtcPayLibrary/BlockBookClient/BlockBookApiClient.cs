using BtcPayLibrary.BlockBookClient.Models;
using BtcPayLibrary.BlockBookClient.Tools;
using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace BtcPayLibrary.BlockBookClient
{
    public class BlockBookApiClient(HttpClient httpClient) : IBlockBookApiClient
    {
        protected readonly HttpClient _httpClient = httpClient;

        private readonly JsonSerializerOptions _serializationOption = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public BlockBookApiClient(string baseUrl)
        : this(new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        })
        {
        }

        public async Task<IEnumerable<Utxo>> GetUtxos(string addressOrXpub, bool confirmed = true)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync($"api/v2/utxo/{addressOrXpub}?{confirmed}");
            response.EnsureSuccessStatusCode();
            return await ReadAsAsync<IEnumerable<Utxo>>(response.Content);
        }
        public async Task<Address> GetAddress(AddressRequest request)
        {
            NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(string.Empty);
            if (request.Page != 0)
            {
                nameValueCollection["page"] = request.Page.ToString();
            }

            if (request.PageSize != 0)
            {
                nameValueCollection["pageSize"] = request.PageSize.ToString();
            }

            if (request.From != null)
            {
                nameValueCollection["from"] = request.From.ToString();
            }

            if (request.To != null)
            {
                nameValueCollection["to"] = request.To.ToString();
            }

            nameValueCollection["details"] = request.Details.ToString();
            if (request.Contract != null)
            {
                nameValueCollection["contract"] = request.Contract.ToString();
            }

            using HttpResponseMessage response = await _httpClient.GetAsync($"api/v2/address/{request.Descriptor}?{nameValueCollection}");
            response.EnsureSuccessStatusCode();
            return await ReadAsAsync<Address>(response.Content);
        }
        public async Task<TransactionResponse> GetTransaction(string TransactionId)
        {
            using var response = await _httpClient.GetAsync($"api/v2/tx/{TransactionId}");
            response.EnsureSuccessStatusCode();
            return await ReadAsAsync<TransactionResponse>(response.Content);
        }

        public async Task<TransactionSpecificResponse> GetTransactionSpecific(string TransactionId)
        {
            using var response = await _httpClient.GetAsync($"api/v2/tx-specific/{TransactionId}");
            response.EnsureSuccessStatusCode();
            var st = await response.Content.ReadAsStringAsync();
            return await ReadAsAsync<TransactionSpecificResponse>(response.Content);
        }
        public async Task<SendTransactionResponse> SendTransaction(string hex)
        {
            using var response = await _httpClient.GetAsync($"api/v2/sendtx/{hex}");
            response.EnsureSuccessStatusCode();
            return await ReadAsAsync<SendTransactionResponse>(response.Content);
        }
        protected async Task<T> ReadAsAsync<T>(HttpContent content)
        {
            try
            {
                using Stream responseStream = await content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<T>(responseStream, _serializationOption);
            }
            catch (JsonException ex)
            {
                throw new BlockBookException(ex.Message, ex.InnerException);
            }
            catch (ArgumentNullException ex2)
            {
                throw new BlockBookException(ex2.Message, ex2.InnerException);
            }
        }
    }
}
