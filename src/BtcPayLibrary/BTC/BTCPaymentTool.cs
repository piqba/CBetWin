using BtcPayLibrary.BlockBookClient;
using BtcPayLibrary.BlockBookClient.Models;
using NBitcoin;

namespace BtcPayLibrary.BTC
{
    public class BTCPaymentTool : IBTCPaymentTool
    {
        private readonly BitcoinSecret _mainBTCSecret;
        private readonly Network _mainNetWork;
        private readonly ScriptPubKeyType _mainScriptPubKeyType;
        private readonly BlockBookApiClient _blockBookApiClient;
        private readonly List<string> _usedUTXOs = [];
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        public BTCPaymentTool(string mainPrivateKey, string network, string scriptPubKey, string blockBookUri)
        {
            switch (network.ToLower())
            {
                case "main":
                    _mainNetWork = Network.Main;
                    _mainBTCSecret = new BitcoinSecret(mainPrivateKey, Network.Main);
                    break;
                default:
                    _mainNetWork = Network.TestNet;
                    _mainBTCSecret = new BitcoinSecret(mainPrivateKey, Network.TestNet);
                    break;
            }
            _mainScriptPubKeyType = scriptPubKey.ToLower() switch
            {
                "legacy" => ScriptPubKeyType.Legacy,
                "segwit" => ScriptPubKeyType.Segwit,
                "segwitp2sh" => ScriptPubKeyType.SegwitP2SH,
                _ => throw new NotImplementedException(),
            };
            _blockBookApiClient = new BlockBookApiClient(blockBookUri);
        }

        public async Task<BlockBookResponse<SendTransactionResponse>> PayToAddress(string addressToWhere, decimal amount, decimal fee = 0.00001m)
        {
            await _semaphoreSlim.WaitAsync();
            var mainAddress = _mainBTCSecret.GetAddress(_mainScriptPubKeyType);
            var mainAddressText = mainAddress.ToString();
            var toWhereBTCAddress = BitcoinAddress.Create(addressToWhere, _mainNetWork);
            var minerFee = new Money(fee, MoneyUnit.BTC);
            var sendAmount = new Money(amount, MoneyUnit.BTC);

            var utxos = await _blockBookApiClient.GetUtxos(mainAddressText);
            if (utxos is null || !utxos.Any())
                return new BlockBookResponse<SendTransactionResponse>
                {
                    Response = null,
                    Completed = false,
                    ErrorMessage = "This wallet has no utxos available."
                };

            var buildTransactionWithUtxoResponse = await BuildTransactionDataWithMainWalletUtxos(utxos, mainAddressText, sendAmount, minerFee);

            if (buildTransactionWithUtxoResponse.TemporalAmount.Satoshi == 0)
                return new BlockBookResponse<SendTransactionResponse>
                {
                    Response = null,
                    Completed = false,
                    ErrorMessage = "This wallet has no utxos available"
                };

            var changeAmount = buildTransactionWithUtxoResponse.TemporalAmount - sendAmount - minerFee;

            if (changeAmount.Satoshi < 0)
                return new BlockBookResponse<SendTransactionResponse>
                {
                    Response = null,
                    Completed = false,
                    ErrorMessage = "This wallet does not have sufficient balance to perform this transaction."
                };

            //amount to return to main wallet
            if (changeAmount.Satoshi > 0)
            {
                var changeTxOut = new TxOut()
                {
                    Value = changeAmount,
                    ScriptPubKey = mainAddress.ScriptPubKey
                };
                buildTransactionWithUtxoResponse.Transaction.Outputs.Add(changeTxOut);
            }

            //amount to sent
            var receiverTxOut = new TxOut()
            {
                Value = sendAmount,
                ScriptPubKey = toWhereBTCAddress.ScriptPubKey
            };

            buildTransactionWithUtxoResponse.Transaction.Outputs.Add(receiverTxOut);
            buildTransactionWithUtxoResponse.Transaction.Sign(_mainBTCSecret, buildTransactionWithUtxoResponse.ICoins);
            var transactionHex = buildTransactionWithUtxoResponse.Transaction.ToHex();

            try
            {
                var broadcastResponse = await _blockBookApiClient.SendTransaction(transactionHex);
                _usedUTXOs.AddRange(buildTransactionWithUtxoResponse.TemporalUseUtxos);
                _semaphoreSlim.Release();
                return broadcastResponse.Result is null ?
                new BlockBookResponse<SendTransactionResponse>
                {
                    Response = null,
                    Completed = false,
                    ErrorMessage = broadcastResponse.Error
                } :
                new BlockBookResponse<SendTransactionResponse>
                {
                    Response = broadcastResponse,
                    Completed = true,
                    ErrorMessage = string.Empty
                };

            }
            catch (HttpRequestException ex)
            {
                return new BlockBookResponse<SendTransactionResponse>
                {
                    Response = null,
                    Completed = false,
                    ErrorMessage = $"Problem with transaction hex {transactionHex} {ex.Message}"
                };
            }
        }
        #region Private Methods
        protected record BuildTransactionWihtUtxoResponse
        {
            public Money TemporalAmount { get; set; }
            public Transaction Transaction { get; set; }
            public List<string> TemporalUseUtxos { get; set; }
            public List<ICoin> ICoins { get; set; }
        }
        private async Task<BuildTransactionWihtUtxoResponse> BuildTransactionDataWithMainWalletUtxos(IEnumerable<Utxo> utxos, string mainAddress, Money sendAmount, Money minerFee)
        {
            var temporalAmount = new Money(0, MoneyUnit.BTC);
            var transaction = Transaction.Create(_mainNetWork);
            var iCoins = new List<ICoin>();
            var temporalUseUtxos = new List<string>();
            foreach (var utxo in utxos)
            {
                if (Convert.ToInt32(utxo.Value) > 0 && !await CheckUtxoInUse(utxo.Txid, mainAddress))
                {
                    temporalAmount += new Money(Convert.ToInt32(utxo.Value), MoneyUnit.Satoshi);

                    var transactionSpecific = await _blockBookApiClient.GetTransactionSpecific(utxo.Txid);
                    var transactionNormalized = await _blockBookApiClient.GetTransaction(utxo.Txid);

                    var positionOutput = transactionNormalized.Vout.SingleOrDefault(r => r.Addresses.Contains(mainAddress)).N;

                    var script = new Script(transactionSpecific.Vout[positionOutput].ScriptPubKey.Asm);

                    transaction.Inputs.Add(new TxIn()
                    {
                        PrevOut = new OutPoint(new uint256(utxo.Txid), positionOutput),
                        ScriptSig = script
                    });

                    var coin = new Coin(new uint256(utxo.Txid), (uint)positionOutput, new Money(Convert.ToInt32(utxo.Value), MoneyUnit.Satoshi), script);
                    iCoins.Add(coin);
                    temporalUseUtxos.Add(utxo.Txid);
                    if (temporalAmount > sendAmount + minerFee)
                        break;
                }
            }
            return new BuildTransactionWihtUtxoResponse
            {
                Transaction = transaction,
                TemporalAmount = temporalAmount,
                TemporalUseUtxos = temporalUseUtxos,
                ICoins = iCoins
            };
        }
        private async Task<bool> CheckUtxoInUse(string utxoId, string senderAddress)
        {
            var detailsSenderAddress = await _blockBookApiClient.GetAddress(new AddressRequest() { Descriptor = senderAddress, Details = Details.txs, PageSize = 100 });
            foreach (var transaction in detailsSenderAddress.Transactions)
            {
                if (transaction.Vin.Exists(v => v.Txid.Equals(utxoId)) && transaction.Vin.Exists(v => Array.Exists(v.Addresses, element => element.Equals(senderAddress))))
                    return true;
            }
            return false;
        }
        #endregion
    }
}
