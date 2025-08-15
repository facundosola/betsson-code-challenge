
using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Services;
using Moq;

namespace Betsson.OnlineWallets.UnitTests
{
    public class OnlineWalletServiceTests
    {
        private readonly Mock<IOnlineWalletRepository> _mockWalletRepo;
        private readonly OnlineWalletService _service;
        public OnlineWalletServiceTests()
        {
            _mockWalletRepo = new Mock<IOnlineWalletRepository>();
            _service = new OnlineWalletService(_mockWalletRepo.Object);
        }

        [Fact, Trait("Method", "DepositFundsAsync")]
        public async Task Deposit_WithNegativeAmount_ShouldThrowException()
        {
            // TODO: Update DepositFundsAsync to validate that the amount is greater than zero and throw an exception if it is not.
            var invalidDeposit = new Deposit { Amount = -100 };
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _service.DepositFundsAsync(invalidDeposit);
            });

        }
        [Fact, Trait("Method", "DepositFundsAsync")]
        public async Task ValidDeposit_IncrementeBalance()
        {
            //arrange - setup mock
            _mockWalletRepo.Setup(Walletrepo => Walletrepo.GetLastOnlineWalletEntryAsync())
                .ReturnsAsync(new OnlineWalletEntry { Amount = 100, BalanceBefore = 0 });

            var transferAmount = new Deposit { Amount = 20 };
            var result = await _service.DepositFundsAsync(transferAmount);
            //Assert: 
            Assert.Equal(120, (result.Amount));
        }

        [Fact, Trait("Method", "GetBalanceAsync")]
        public async Task GetBalance_WhitNoTransactions_ReturnsZero()
        {
            //Arrange: Mock the wallet to return null, simulating no transactions.
            _mockWalletRepo.Setup(walletRepo => walletRepo.GetLastOnlineWalletEntryAsync()).ReturnsAsync((OnlineWalletEntry?)null);

            //Act: call the service method 
            var balance = await _service.GetBalanceAsync();

            //Assert: Verify that returns balance is 0 
            Assert.Equal(0, balance.Amount);

        }


        [Fact, Trait("Method", "GetBalanceAsync")]
        public async Task GetBalance_WhenWalletHasTransaction_ReturnsSumOfBalanceBeforeAndAmount()
        {
            //Arrange: Mock a wallet entry with transaction
            var walletEntry = new OnlineWalletEntry { Amount = 100, BalanceBefore = 50 };
            _mockWalletRepo.Setup(Walletrepo => Walletrepo.GetLastOnlineWalletEntryAsync()).ReturnsAsync(walletEntry);

            //Act: Call the service method 
            var balance = await _service.GetBalanceAsync();

            //Assert: The balance should be the sum of BalanceBefore and Amount
            Assert.Equal(150, balance.Amount);
        }


        public async Task DepositFundsAsyn_WithDecimalAmount_UpdatesBalanceCorrectly()
        {
            //assert 
            //_mockWalletRepo.Setup();

            //act 

            //arrange
        }


    }
}