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

        [Fact, Trait("POST", "onlineWallet/withdraw")]
        public async Task Withdraw_WithSufficientFunds_DecrementsBalanceCorrectly()
        {

            var depositAmount = 179989.45m;
            var depositRequest = new DepositRequest { Amount = depositAmount };
            var depositResponse = await _client.PostAsJsonAsync("/onlinewallet/deposit", depositRequest);
            depositResponse.EnsureSuccessStatusCode();

            var currentBalanceResponse = await _client.GetFromJsonAsync<BalanceResponse>("/onlinewallet/balance");
            Assert.NotNull(currentBalanceResponse);

            var withdrawAmount = 115000.37m;
            var withdrawRequest = new WithdrawalRequest { Amount = withdrawAmount };
            var expectedBalance = currentBalanceResponse.Amount - withdrawAmount;
    

            var withdrawResponse = await _client.PostAsJsonAsync("/onlinewallet/withdraw", withdrawRequest);
            withdrawResponse.EnsureSuccessStatusCode();

            var finalBalanceResponse = await withdrawResponse.Content.ReadFromJsonAsync<BalanceResponse>();
            Assert.NotNull(finalBalanceResponse);
            Assert.Equal(expectedBalance, finalBalanceResponse.Amount);

        }
        [Fact, Trait("POST", "onlineWallet/withdraw")]
        public async Task Withdraw_WithInsufficientFunds_ReturnsBadRequest()
        {
            // Arrange
            var withdrawAmount = 1000000m; // Assuming this amount exceeds the current balance
            var withdrawRequest = new WithdrawalRequest { Amount = withdrawAmount };
            var expectedErrorMessage = "Invalid withdrawal amount. There are insufficient funds.";

            // Act
            var response = await _client.PostAsJsonAsync("/onlinewallet/withdraw", withdrawRequest);
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
            var problemDetails = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedErrorMessage, problemDetails);
        }
        [Fact, Trait("POST", "onlineWallet/withdraw")]
        public async Task Withdraw_WithNegativeAmount_ReturnsBadRequest()
        {
            // Arrange
            var negativeWithdrawAmount = -500.00m;
            var withdrawRequest = new WithdrawalRequest { Amount = negativeWithdrawAmount };
            // Act
            var response = await _client.PostAsJsonAsync("/onlinewallet/withdraw", withdrawRequest);
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        }
        [Fact]
        public async Task Withdraw_WithZeroAmount_ReturnsSuccessAndDoesNotChangeBalance()
        {
            // Arrange
           
            var initialDeposit = 50m;
            await _client.PostAsJsonAsync("/onlinewallet/deposit", new DepositRequest { Amount = initialDeposit });

            var initialBalanceResponse = await _client.GetFromJsonAsync<BalanceResponse>("/onlinewallet/balance");
            Assert.NotNull(initialBalanceResponse);
            var expectedBalance = initialBalanceResponse.Amount;

            var zeroWithdrawalRequest = new WithdrawalRequest { Amount = 0 };

            // Act
            var response = await _client.PostAsJsonAsync("/onlinewallet/withdraw", zeroWithdrawalRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var finalBalanceResponse = await response.Content.ReadFromJsonAsync<BalanceResponse>();
            Assert.NotNull(finalBalanceResponse);

            Assert.Equal(expectedBalance, finalBalanceResponse.Amount);
        }
    }

}
