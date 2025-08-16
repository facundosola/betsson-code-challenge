using Betsson.OnlineWallets.Web.Models;
using Microsoft.AspNetCore.Mvc.Testing;
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

            var response = await _client.GetAsync("/onlinewallet/balance");
            response.EnsureSuccessStatusCode();

            var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            Assert.NotNull(balance);
            Assert.True(balance.Amount >= 0);
        }

        [Fact, Trait("GET", "onlineWallet/balance")]
        public async Task GetBalance_AfterDeposit_ShoulBeCorrect()
        {
            //Arrange            
            var initialBalance = await _client.GetFromJsonAsync<BalanceResponse>("/onlinewallet/balance");
            Assert.NotNull(initialBalance);
            var depositRequest = new DepositRequest { Amount = 1515.75m };

            //Act
            var depositResponse = await _client.PostAsJsonAsync("/onlinewallet/deposit", depositRequest);
            depositResponse.EnsureSuccessStatusCode();
            var response = await _client.GetAsync("/onlinewallet/balance");
            response.EnsureSuccessStatusCode();
            var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();

            //Assert
            Assert.NotNull(balance);
            var expectedBalance = initialBalance.Amount + depositRequest.Amount;
            Assert.Equal(expectedBalance, balance.Amount);

        }

        [Fact, Trait("POST", "onlineWallet/deposit")]
        public async Task DepositFunds_WithValidAmount_ProcessCorrectly()
        {

            //Arrange
            var initialBalanceResponse = await _client.GetFromJsonAsync<BalanceResponse>("/onlinewallet/balance");
            Assert.NotNull(initialBalanceResponse);
            decimal depositAmount = 15750.75m;
            var depositRequest = new DepositRequest { Amount = depositAmount };
            var expectedBalance = initialBalanceResponse.Amount + depositAmount;

            //Act
            var response = await _client.PostAsJsonAsync("/onlinewallet/deposit", depositRequest);
            response.EnsureSuccessStatusCode();

            var balance = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            Assert.NotNull(balance);
            Assert.Equal(expectedBalance, balance.Amount);

        }

        [Fact, Trait("POST", "onlineWallet/deposit")]
        public async Task DepositFunds_WithNegativeAmout_ReturnsBadRequest()
        {
            //Arrange
            decimal negativeDepositAmount = -1500.00m;
            var depositRequest = new DepositRequest { Amount = negativeDepositAmount };
            //Act
            var response = await _client.PostAsJsonAsync("/onlinewallet/deposit", depositRequest);

            //Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        }

        [Fact, Trait("POST", "onlineWallet/deposit")]
        public async Task DepositFunds_WithZeroAmount_ProcessCorrectly()
        {
            var depositAmount = 0m;
            var depositRequest = new DepositRequest { Amount = depositAmount };

            var response = await _client.PostAsJsonAsync("/onlinewallet/deposit", depositRequest);
            response.EnsureSuccessStatusCode();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        }

   
    }
    
}
