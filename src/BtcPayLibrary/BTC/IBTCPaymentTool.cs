using BtcPayLibrary.BlockBookClient.Models;
using BtcPayLibrary.BlockBookClient.Tools;

namespace BtcPayLibrary.BTC
{
    public interface IBTCPaymentTool
    {
        Task<BlockBookResponse<SendTransactionResponse>> PayToAddress(string addressToWhere, decimal amount, decimal fee = 0.00001m);
    }
}
