using BtcPayLibrary.BlockBookClient.Models;

namespace BtcPayLibrary.BTC
{
    public interface IBTCPaymentTool
    {
        Task<BlockBookResponse<SendTransactionResponse>> PayToAddress(string addressToWhere, decimal amount, decimal fee = 0.00001m);
    }
}
