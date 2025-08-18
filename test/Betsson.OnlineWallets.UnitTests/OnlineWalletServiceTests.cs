
using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Exceptions;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Services;
using Moq;

namespace Betsson.OnlineWallets.UnitTests
{
    public class OnlineWalletServiceTests
    {
        private readonly Mock<IOnlineWalletRepository> _mockWalletRepo;
        private readonly OnlineWalletService _service;

       private void SetupWalletEntry(decimal? balanceBefore = null, decimal? amount = null)
{
        _mockWalletRepo.Setup(r => r.GetLastOnlineWalletEntryAsync())
        .ReturnsAsync(new OnlineWalletEntry
        {
            BalanceBefore = balanceBefore ?? 0, //if balanceBefore is null, use 0
            Amount = amount ?? 0       //if amount is null, use 0
        });
}   
        public OnlineWalletServiceTests()
        {
            _mockWalletRepo = new Mock<IOnlineWalletRepository>();
            _service = new OnlineWalletService(_mockWalletRepo.Object);
        }

        [Fact, Trait("Method", "DepositFundsAsync")]
        public async Task Deposit_WithDecimals_ProcessCorrectly()
        {
            //Arrange
            var initialBalance = 197.13344m;
            SetupWalletEntry(initialBalance);
         
            //Act
            var transferAmount = new Deposit { Amount = 524.74m };
            var actualBalance = await _service.DepositFundsAsync(transferAmount);

            //Assert: 
            var expectedBalance = initialBalance + transferAmount.Amount;
            Assert.Equal(expectedBalance, (actualBalance.Amount));
        }

        [Fact, Trait("Method", "DepositFundsAsync")]
        public async Task DepositFunds_WithNegativeAmount_ShouldThrowException()
        {

            // Validation is performed at the API layer, but it's important to have a unit test here.
            // TODO: Update DepositFundsAsync to validate that the amount is greater than zero and throw an exception if it is not.
            var invalidDeposit = new Deposit { Amount = -100 };
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _service.DepositFundsAsync(invalidDeposit);
            });

        }

        [Fact, Trait("Method", "DepositFundsAsync")]
        public async Task DepositFundsAsync_WithValidAmount_IncrementsBalanceCorrectly()
        {
            //Arrange
            var initialBalance = 300m;
            SetupWalletEntry(initialBalance);

            //Act
            var transferAmount = new Deposit { Amount = 20m };
            var actualBalance = await _service.DepositFundsAsync(transferAmount);

            //Assert 
            var expectedBalance = initialBalance + transferAmount.Amount;
            Assert.Equal(expectedBalance, actualBalance.Amount);
        }

        //overflow exception
        [Fact, Trait("Method", "DepositFundsAsync")]
        public async Task DepositFundsAsync_WithMaxValue_ShouldThrownOverflowException()
        {
            var initialBalance = decimal.MaxValue - 1;
            SetupWalletEntry(initialBalance);
            var transferAmount = new Deposit { Amount = 2 };
            await Assert.ThrowsAsync<OverflowException>(async () =>
            {
                await _service.DepositFundsAsync(transferAmount);
            });
        }

        [Fact, Trait("Method", "DepositFundsAsync")]
        public async Task DepositFundsAsync_WithExactMaxValue_ShouldSucced()
        {
           SetupWalletEntry(balanceBefore: null, amount: null);
            var deposit = new Deposit { Amount = decimal.MaxValue };
            var result = await _service.DepositFundsAsync(deposit);
            Assert.Equal(decimal.MaxValue, result.Amount);
        }


        [Fact, Trait("Method", "GetBalanceAsync")]
        public async Task GetBalance_WhenNoTransactionsExist_ReturnsZero()
        {

            //Arrange: Mock the wallet to return null, simulating no transactions.
           SetupWalletEntry(balanceBefore: null, amount: null);
            var expectedBalance = 0;

            //Act: call the service method 
            var actualBalance = await _service.GetBalanceAsync();

            //Assert: Verify that returns balance is 0 
            Assert.Equal(expectedBalance, actualBalance.Amount);

        }

        [Fact(), Trait("Method", "GetBalanceAsync")]
        public async Task GetBalance_WhenWalletHasTransaction_ReturnsSumOfBalanceBeforeAndAmount()
        {
            var _amount = 100;
            var _balanceBefore = 50;

            //Arrange: Mock a wallet entry with transaction
            SetupWalletEntry(_amount, _balanceBefore);

            //Act: Call the service method 
            var balance = await _service.GetBalanceAsync();

            //Assert: The balance should be the sum of BalanceBefore and Amount
            var expectedBalance = _amount + _balanceBefore;
            Assert.Equal(expectedBalance, balance.Amount);
        }

        [Fact, Trait("Method", "WithdrawFundsAsync")]
        public async Task Withdrawal_DecrementsBalanceCorrectly()
        {
            //happy path
            //Arrange: Mock a wallet with initial balance 
            var _balanceBefore = 4500.68m;
            var withdrawalAmount = new Withdrawal { Amount = 1500.33m };
            var expectedBalance = _balanceBefore - withdrawalAmount.Amount;

            // var lastWalletEntry = new OnlineWalletEntry
            // {
            //     Amount = 0,
            //     BalanceBefore = _balanceBefore
            // };

            SetupWalletEntry(_balanceBefore);
            //_mockWalletRepo.Setup(walletRepo => walletRepo.GetLastOnlineWalletEntryAsync()).ReturnsAsync(lastWalletEntry);


            //Act: Call the service method
            var result = await _service.WithdrawFundsAsync(withdrawalAmount);

            //Assert: Validates expected balance value
            Assert.Equal(expectedBalance, result.Amount);
            //Verify that the wallet entry was inserted with the correct values
            _mockWalletRepo.Verify(walletRepo => walletRepo.InsertOnlineWalletEntryAsync(It.Is<OnlineWalletEntry>(entry => entry.Amount == -withdrawalAmount.Amount && entry.BalanceBefore == _balanceBefore)), Times.Once);

        }

        [Fact, Trait("Method", "WithdrawFundsAsync")]
        public async Task Withdrawal_WithInsufficientFunds_ThrowsException()
        {

            //Arrange: 
            var initialBalance = 375.80m;
            SetupWalletEntry(balanceBefore: initialBalance);
            // var walletEntry = new OnlineWalletEntry { Amount = 375.80m };
            // _mockWalletRepo.Setup(walletRepo => walletRepo.GetLastOnlineWalletEntryAsync()).ReturnsAsync(walletEntry);

            var withdrawAmount = new Withdrawal { Amount = 550.55m };

            //Act
            var exception = await Assert.ThrowsAsync<InsufficientBalanceException>(async () =>
            {
                await _service.WithdrawFundsAsync(withdrawAmount);
            });

            //Assert
            Assert.Equal("Invalid withdrawal amount. There are insufficient funds.", exception.Message);

        }

        [Fact, Trait("Method", "WithdrawFundsAsync")]
        public async Task Withdrawal_WithZeroAmount()
        {
            //Arrange
            var initialBalance = 488.111m;
            SetupWalletEntry(balanceBefore: initialBalance);
            // _mockWalletRepo.Setup(walletRepo => walletRepo.GetLastOnlineWalletEntryAsync())
            //     .ReturnsAsync(new OnlineWalletEntry { Amount = 0, BalanceBefore = initialBalance });

            var zeroWithdrawal = new Withdrawal { Amount = 0 };

            //Act
            var result = await _service.WithdrawFundsAsync(zeroWithdrawal);

            //Assert
            Assert.Equal(initialBalance, result.Amount);

            _mockWalletRepo.Verify(walletRepo => walletRepo.InsertOnlineWalletEntryAsync(It.IsAny<OnlineWalletEntry>()), Times.Once);
        }

        //Negative amount for withdraw
        [Fact, Trait("Method", "WithdrawFundsAsync")]
        public async Task Withdrawal_WithNegativeAmount_ThrowsException()
        {
            //Arrange
            var negativeWithdrawAmount = new Withdrawal { Amount = -14000 };

            //Act & Assert
            // Validation is performed at the API layer, but it's important to have a unit test here. Same case that DepositFundsAsync 
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await _service.WithdrawFundsAsync(negativeWithdrawAmount);
            });
        }

        [Fact, Trait("Method", "WithdrawFundsAsync")]
        public async Task WithdrawFundsAsync_WithExactBalance_ShouldSucceedAndReturnZeroBalance()
        {
            // Arrange
            var initialBalance = 100;
            var withdrawalAmount = new Withdrawal { Amount = 100 };
            SetupWalletEntry(balanceBefore: initialBalance, amount: null);

            // _mockWalletRepo.Setup(repo => repo.GetLastOnlineWalletEntryAsync())
            //          .ReturnsAsync(new OnlineWalletEntry { Amount = initialBalance, BalanceBefore = 0 });

            // Act
            var newBalance = await _service.WithdrawFundsAsync(withdrawalAmount);

            // Assert

            _mockWalletRepo.Verify(repo => repo.InsertOnlineWalletEntryAsync(
                    It.Is<OnlineWalletEntry>(entry => entry.Amount == -initialBalance && entry.BalanceBefore == initialBalance)), Times.Once);


            Assert.Equal(0, newBalance.Amount);

        }

        [Fact, Trait("Method", "WithdrawFundsAsync")]
        public async Task WithdrawFundsAsync_WithMaxValueBalanceAndFullWithdrawal_ShouldSucced()
        {
            SetupWalletEntry(balanceBefore: decimal.MaxValue);
            // _mockWalletRepo.Setup(mwr => mwr.GetLastOnlineWalletEntryAsync())
            //     .ReturnsAsync(new OnlineWalletEntry { BalanceBefore = decimal.MaxValue });
            var withdrawal = new Withdrawal { Amount = decimal.MaxValue };
            var result = await _service.WithdrawFundsAsync(withdrawal);
            Assert.Equal(0, result.Amount);

        }
    }
}