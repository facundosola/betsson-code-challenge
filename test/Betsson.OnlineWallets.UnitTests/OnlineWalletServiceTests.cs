
using Betsson.OnlineWallets.Data.Models;
using Betsson.OnlineWallets.Data.Repositories;
using Betsson.OnlineWallets.Models;
using Betsson.OnlineWallets.Services;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Betsson.OnlineWallets.UnitTests
{
    public class OnlineWalletServiceTests
    {
        private readonly Mock<IOnlineWalletRepository> _mockRepo;
        private readonly OnlineWalletService _service;
        public OnlineWalletServiceTests()
        {
            _mockRepo = new Mock<IOnlineWalletRepository>();
            _service = new OnlineWalletService(_mockRepo.Object);
        }
        [Fact]
        public async Task Deposit_WithNegativeAmount_ShouldThrowException()
        {   
            
            var invalidDeposit = new Deposit { Amount = -100 };

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => 
            {
                await _service.DepositFundsAsync(invalidDeposit);
            });

        }
    }
}