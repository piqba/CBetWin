using BtcPayLibrary.BTC;
using NBitcoin;

namespace BTCPayLibrary.UnitTests
{
    public class BTCPaymentToolTests
    {
        [Fact]
        public async Task SendTransactionAndReceiveCompleteStatus()
        {
            var btcPayLibrary = new BTCPaymentTool(mainPrivateKey: "cQrZH9QPJPc7Q9iesd7fw4FCVh6j8PuiH2uxJoGNJHsQXs6jH4Z7", network: "testnet", scriptPubKey: "legacy", blockBookUri: "https://go.getblock.io/0932ac5772f74d85a697647ad074e826/");
            /*new wallet*/
            var privateKey = new Key();
            var bitcoinPrivateKey = privateKey.GetWif(Network.TestNet);
            var address = bitcoinPrivateKey.GetAddress(ScriptPubKeyType.Legacy);

            var amount = 0.000017m;//1 usd more or less
            var response = await btcPayLibrary.PayToAddress(address.ToString(), amount);
            Assert.NotNull(response);
            Assert.True(response.Completed);
        }
    }
}