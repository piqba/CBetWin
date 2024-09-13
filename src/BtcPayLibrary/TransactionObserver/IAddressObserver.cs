using BtcPayLibrary.BlockBookClient.Models;
using BtcPayLibrary.BlockBookClient.Tools;
using BtcPayLibrary.TransactionObserver.Models;

namespace BtcPayLibrary.TransactionObserver
{
    public interface IAddressObserver
    {
        Task<BlockBookResponse<TransactionObserved>> WaitForTransactionByAddress(ObserveAddressRequest addressRequest, string senderAddress, CancellationToken cancellationToken);
        Task<BlockBookResponse<TransactionSpecificResponse>> WaitForTransactionByConfirmationNumber(ConfirmTransactionRequest confirmTransactionRequest, CancellationToken cancellationToken);
    }
}
