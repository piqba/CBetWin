using BtcPayLibrary.BlockBookClient;
using BtcPayLibrary.BlockBookClient.Models;
using BtcPayLibrary.BlockBookClient.Tools;
using BtcPayLibrary.TransactionObserver.Models;
using Microsoft.Extensions.Logging;

namespace BtcPayLibrary.TransactionObserver
{
    public class AddressObserver(string blockBookUri, ILogger<AddressObserver> logger) : IAddressObserver
    {
        private readonly ILogger<AddressObserver> _logger = logger;
        private readonly BlockBookApiClient _blockBookApiClient = new(blockBookUri);

        public async Task<BlockBookResponse<TransactionObserved>> WaitForTransactionByAddress(ObserveAddressRequest addressRequest, string senderAddress, CancellationToken cancellationToken)
        {
            if (addressRequest.Timeout <= TimeSpan.Zero)
                addressRequest.Timeout = TimeSpan.FromMinutes(5.0);

            _logger.LogInformation("WaitForTransactionByAddress {Address}. Timeout: {Timeout}", addressRequest.Address, addressRequest.Timeout);
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationTokenSource.Token, cancellationToken);

            var timer = new Timer(delegate
            {
                timeoutCancellationTokenSource.Cancel();
            }, null, addressRequest.Timeout, Timeout.InfiniteTimeSpan);

            while (!linkedCts.IsCancellationRequested)
            {
                var addressInfo = await _blockBookApiClient.GetAddress(new AddressRequest() { Descriptor = senderAddress, Details = Details.txs, PageSize = 100 });
                var foundTransaction = addressInfo.Transactions
                                        .FirstOrDefault(t => t.Vout.Any(v => v.Addresses.Contains(addressRequest.Address)));
                if (foundTransaction != null)
                {
                    var specificTransaction = await _blockBookApiClient.GetTransactionSpecific(foundTransaction.Txid);
                    if (specificTransaction != null)
                    {
                        var foundVout = specificTransaction.Vout.FirstOrDefault(v => v.ScriptPubKey.Address.Equals(addressRequest.Address));
                        if (foundVout != null)
                        {
                            return new BlockBookResponse<TransactionObserved>
                            {
                                Response = new TransactionObserved
                                {
                                    PaidAmount = (double)foundVout.Value,
                                    Txid = foundTransaction.Txid
                                },
                                Completed = true
                            };
                        }
                    }
                }
            }
            return new BlockBookResponse<TransactionObserved>
            {
                Completed = false
            };
        }
        public async Task<BlockBookResponse<TransactionSpecificResponse>> WaitForTransactionByConfirmationNumber(ConfirmTransactionRequest confirmTransactionRequest, CancellationToken cancellationToken)
        {
            if (confirmTransactionRequest.Timeout <= TimeSpan.Zero)
                confirmTransactionRequest.Timeout = TimeSpan.FromMinutes(5.0);

            _logger.LogInformation("WaitForTransactionByConfirmationNumber {TxId}. Timeout: {Timeout}", confirmTransactionRequest.TxId, confirmTransactionRequest.Timeout);
            var timeoutCancellationTokenSource = new CancellationTokenSource();
            using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCancellationTokenSource.Token, cancellationToken);

            var timer = new Timer(delegate
            {
                timeoutCancellationTokenSource.Cancel();
            }, null, confirmTransactionRequest.Timeout, Timeout.InfiniteTimeSpan);

            while (!linkedCts.IsCancellationRequested)
            {
                var specificTransaction = await _blockBookApiClient.GetTransactionSpecific(confirmTransactionRequest.TxId);
                if (specificTransaction != null)
                {
                    if (specificTransaction.Confirmations >= confirmTransactionRequest.Confirmations)
                        return new BlockBookResponse<TransactionSpecificResponse>
                        {
                            Response = specificTransaction,
                            Completed = true
                        };
                }
            }
            return new BlockBookResponse<TransactionSpecificResponse>
            {
                Completed = false
            };
        }
    }
}
