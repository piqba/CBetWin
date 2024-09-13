using BtcPayLibrary.BlockBookClient.Models;

namespace BtcPayLibrary.BlockBookClient
{
    public interface IBlockBookApiClient
    {
        Task<IEnumerable<Utxo>> GetUtxos(string addressOrXpub, bool confirmed = true);
        Task<Address> GetAddress(AddressRequest request);
        Task<TransactionResponse> GetTransaction(string TransactionId);
        Task<TransactionSpecificResponse> GetTransactionSpecific(string TransactionId);
        Task<SendTransactionResponse> SendTransaction(string hex);
    }
}
