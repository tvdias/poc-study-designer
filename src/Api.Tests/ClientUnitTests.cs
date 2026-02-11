using Api.Features.Clients;
using Api.Features.Clients.Validators;

namespace Api.Tests;

public class ClientUnitTests
{
    [Fact]
    public void CreateClientRequest_CreatesInstance()
    {
        var accountName = "Test Client";
        var companyNumber = "123456";
        var customerNumber = "CUST-001";
        var companyCode = "TC";
        var request = new CreateClientRequest(accountName, companyNumber, customerNumber, companyCode);
        Assert.Equal(accountName, request.AccountName);
        Assert.Equal(companyNumber, request.CompanyNumber);
        Assert.Equal(customerNumber, request.CustomerNumber);
        Assert.Equal(companyCode, request.CompanyCode);
    }
}

public class CreateClientValidatorTests
{
    private readonly CreateClientValidator _validator = new();

    [Fact]
    public async Task ValidClient_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateClientRequest("Valid Account Name", "123456", "CUST-001", "VAL");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyAccountName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateClientRequest("", null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Account name is required.");
    }

    [Fact]
    public async Task NullAccountName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateClientRequest(null!, null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Account name is required.");
    }

    [Fact]
    public async Task AccountNameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new CreateClientRequest(longName, null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Account name must not exceed 200 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task AccountNameExactly200Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 200);
        var request = new CreateClientRequest(maxLengthName, null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task WhitespaceAccountName_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateClientRequest("   ", null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Account name is required.");
    }

    [Fact]
    public async Task EmptyCustomerNumber_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateClientRequest("Valid Name", null, "", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Customer number is required.");
    }

    [Fact]
    public async Task NullCustomerNumber_ShouldFailValidation()
    {
        // Arrange
        var request = new CreateClientRequest("Valid Name", null, null!, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Customer number is required.");
    }

    [Fact]
    public async Task CompanyNumberExceeding50Characters_ShouldFailValidation()
    {
        // Arrange
        var longNumber = new string('1', 51);
        var request = new CreateClientRequest("Valid Name", longNumber, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Company number must not exceed 50 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task CustomerNumberExceeding50Characters_ShouldFailValidation()
    {
        // Arrange
        var longNumber = new string('1', 51);
        var request = new CreateClientRequest("Valid Name", null, longNumber, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Customer number must not exceed 50 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task CompanyCodeExceeding50Characters_ShouldFailValidation()
    {
        // Arrange
        var longCode = new string('A', 51);
        var request = new CreateClientRequest("Valid Name", null, "CUST-001", longCode);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Company code must not exceed 50 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task NullOptionalFields_ShouldPassValidation()
    {
        // Arrange
        var request = new CreateClientRequest("Valid Account Name", null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

public class UpdateClientValidatorTests
{
    private readonly UpdateClientValidator _validator = new();

    [Fact]
    public async Task ValidClient_ShouldPassValidation()
    {
        // Arrange
        var request = new UpdateClientRequest("Updated Account Name", "123456", "CUST-001", "UPD");

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task EmptyAccountName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateClientRequest("", null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Account name is required.");
    }

    [Fact]
    public async Task NullAccountName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateClientRequest(null!, null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Account name is required.");
    }

    [Fact]
    public async Task AccountNameExceeding200Characters_ShouldFailValidation()
    {
        // Arrange
        var longName = new string('a', 201);
        var request = new UpdateClientRequest(longName, null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Account name must not exceed 200 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task AccountNameExactly200Characters_ShouldPassValidation()
    {
        // Arrange
        var maxLengthName = new string('a', 200);
        var request = new UpdateClientRequest(maxLengthName, null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task WhitespaceAccountName_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateClientRequest("   ", null, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Account name is required.");
    }

    [Fact]
    public async Task EmptyCustomerNumber_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateClientRequest("Valid Name", null, "", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Customer number is required.");
    }

    [Fact]
    public async Task NullCustomerNumber_ShouldFailValidation()
    {
        // Arrange
        var request = new UpdateClientRequest("Valid Name", null, null!, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Customer number is required.");
    }

    [Fact]
    public async Task CompanyNumberExceeding50Characters_ShouldFailValidation()
    {
        // Arrange
        var longNumber = new string('1', 51);
        var request = new UpdateClientRequest("Valid Name", longNumber, "CUST-001", null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Company number must not exceed 50 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task CustomerNumberExceeding50Characters_ShouldFailValidation()
    {
        // Arrange
        var longNumber = new string('1', 51);
        var request = new UpdateClientRequest("Valid Name", null, longNumber, null);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Customer number must not exceed 50 characters.", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task CompanyCodeExceeding50Characters_ShouldFailValidation()
    {
        // Arrange
        var longCode = new string('A', 51);
        var request = new UpdateClientRequest("Valid Name", null, "CUST-001", longCode);

        // Act
        var result = await _validator.ValidateAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Company code must not exceed 50 characters.", result.Errors[0].ErrorMessage);
    }
}
