using Betsson.OnlineWallets.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using Betsson.OnlineWallets.Web;



namespace Betsson.OnlineWallets.ApiTests
{
    public class OnlineWalletControllerApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public OnlineWalletControllerApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
       
        }

        [Fact, Trait("GET", "onlineWallet/balance")]
        public async Task GetBalance_ReturnsOkAndZeroBalanceInitially()
        {
            decimal initialBalance = 0m;
            var response = await _client.GetAsync("/onlinewallet/balance");
            response.EnsureSuccessStatusCode();

            var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            Assert.NotNull(balance);
            Assert.Equal(initialBalance, balance.Amount);
        }

        [Fact, Trait("POST", "onlineWallet/deposit")]
        public async Task DepositFunds_WithValidAmount_IncrementsBalanceCorrectly()
        {

            var initialBalance = await _client.GetFromJsonAsync<BalanceResponse>("/onlinewallet/balance");
            Assert.NotNull(initialBalance);

            var depositRequest = new DepositRequest { Amount = 100m };
            var response = await _client.PostAsJsonAsync("/onlinewallet/deposit", depositRequest);
            response.EnsureSuccessStatusCode();
            var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            Assert.NotNull(balance);
            var expectedBalance = initialBalance.Amount + depositRequest.Amount;
            Assert.Equal(expectedBalance, balance.Amount);

        }
       

    }
}
