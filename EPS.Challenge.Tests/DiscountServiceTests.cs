using EPS.Challenge;
using EPS.Challenge.Model;
using EPS.Challenge.Repositories.Interfaces;
using EPS.Challenge.Services;
using Moq;

public class DiscountServiceTests
{
    private readonly Mock<IDiscountRepository> _repository;
    private readonly DiscountService _service;

    public DiscountServiceTests()
    {
        _repository = new Mock<IDiscountRepository>();
        _service = new DiscountService(_repository.Object);
    }

    [Theory]
    [InlineData(1, 8)]
    [InlineData(10, 7)]
    [InlineData(100, 8)]
    [InlineData(1000, 7)]
    [InlineData(2000, 8)]
    public async Task GenerateCodes_ShouldGenerateUniqueCodes(int count, int length)
    {
        //Arrange
        _repository.Setup(x => x.SaveCodeToDatabase(It.IsAny<string>())).Returns(true);
        var request = new GenerateRequest { Count = count, Length = length };

        //Act
        var response = await _service.GenerateCodes(request, null);

        //Assert
        Assert.True(response.Result);
        Assert.Equal(count, response.Codes.Count);
        Assert.Equal(count, response.Codes.Distinct().Count());
        _repository.Verify(x => x.SaveCodeToDatabase(It.IsAny<string>()), Times.Exactly(count));

    }

    [Fact]
    public async Task GenerateDiscountCodes_ShouldReturnFalse_WhenCountExceedsLimit()
    {
        // Arrange
        var request = new GenerateRequest { Count = 2001, Length = 7 };

        //Act
        var response = await _service.GenerateCodes(request, null);

        //Assert
        Assert.False(response.Result);
        _repository.Verify(x => x.SaveCodeToDatabase(It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task GenerateDiscountCodes_ShouldReturnFalse_WhenLengthIsInvalid()
    {
        // Arrange
        _repository.Setup(x => x.SaveCodeToDatabase(It.IsAny<string>())).Returns(true);
        var request = new GenerateRequest { Count = 1, Length = 5 };

        //Act
        var response = await _service.GenerateCodes(request, null);

        //Assert
        Assert.False(response.Result);
        _repository.Verify(x => x.SaveCodeToDatabase(It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task UseCode_ShouldReturn0_WhenCodeExistsAndNotUsed()
    {
        // Arrange
        var code = "TESTCODE";
        _repository.Setup(x => x.GetActiveCode(It.IsAny<string>())).Returns(new DiscountCode { Code = code, IsUsed = false });
        _repository.Setup(x => x.MarkCodeAsUsed(It.IsAny<string>())).Returns(true);

        //Act
        var useResponse = await _service.UseCode(new UseCodeRequest { Code = code }, null);

        //Assert
        Assert.Equal(0, useResponse.Result);
        _repository.Verify(x => x.GetActiveCode(code), Times.Once());
        _repository.Verify(x => x.MarkCodeAsUsed(code), Times.Once());
    }

    [Fact]
    public async Task UseCode_ShouldReturn1_WhenCodeExistsAndIsUsed()
    {
        // Arrange
        var code = "TESTCODE";
        _repository.Setup(x => x.GetActiveCode(It.IsAny<string>())).Returns(new DiscountCode { Code = code, IsUsed = true });

        //Act
        var useResponse = await _service.UseCode(new UseCodeRequest { Code = code }, null);

        //Assert
        Assert.Equal(1, useResponse.Result);
        _repository.Verify(x => x.GetActiveCode(code), Times.Once());
        _repository.Verify(x => x.MarkCodeAsUsed(code), Times.Never());
    }

    [Fact]
    public async Task UseCode_ShouldReturn2_WhenCodeNotExists()
    {
        // Arrange
        var code = "TESTCODE";
        _repository.Setup(x => x.GetActiveCode(It.IsAny<string>())).Returns((DiscountCode?)null);

        //Act
        var useResponse = await _service.UseCode(new UseCodeRequest { Code = code }, null);

        //Assert
        Assert.Equal(2, useResponse.Result); 
        _repository.Verify(x => x.GetActiveCode(code), Times.Once());
        _repository.Verify(x => x.MarkCodeAsUsed(code), Times.Never());
    }
}
