using Backend.Services;
using Common.Security;
using FluentAssertions;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Contracts.Auth;
using Shared.Domain;

namespace Backend.Tests.Services;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_Should_Return_Tokens_When_Credentials_Valid()
    {
        await using var db = CreateDbContext();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user1",
            Email = "user1@test.local",
            IsActive = true
        };

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByNameAsync("user1")).ReturnsAsync(user);
        userManager.Setup(x => x.CheckPasswordAsync(user, "Pass123!")).ReturnsAsync(true);
        userManager.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { AuthConstants.WorkerRole });

        var service = CreateAuthService(userManager.Object, db);

        var result = await service.LoginAsync(new LoginRequest { Login = "user1", Password = "Pass123!" });

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        (await db.RefreshTokens.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RefreshAsync_Should_Rotate_RefreshToken()
    {
        await using var db = CreateDbContext();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "user2",
            IsActive = true
        };
        db.Users.Add(user);
        var oldToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = "old-token",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(5)
        };
        db.RefreshTokens.Add(oldToken);
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { AuthConstants.SupervisorRole });

        var service = CreateAuthService(userManager.Object, db);

        var result = await service.RefreshAsync("old-token");

        result.Should().NotBeNull();
        var storedOld = await db.RefreshTokens.SingleAsync(x => x.Token == "old-token");
        storedOld.IsRevoked.Should().BeTrue();
        storedOld.ReplacedByToken.Should().NotBeNullOrWhiteSpace();
        (await db.RefreshTokens.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_Should_Revoke_Existing_Token()
    {
        await using var db = CreateDbContext();
        db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "revokable",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        });
        await db.SaveChangesAsync();

        var service = CreateAuthService(CreateUserManagerMock().Object, db);

        await service.RevokeRefreshTokenAsync("revokable");

        var token = await db.RefreshTokens.SingleAsync(x => x.Token == "revokable");
        token.IsRevoked.Should().BeTrue();
    }

    private static AuthService CreateAuthService(UserManager<ApplicationUser> userManager, AppDbContext db)
    {
        var jwt = Options.Create(new JwtSettings
        {
            Issuer = "TechWorkFlow",
            Audience = "TechWorkFlow.Client",
            SigningKey = "unit-test-signing-key-at-least-32-chars",
            AccessTokenMinutes = 30,
            RefreshTokenDays = 7
        });

        return new AuthService(userManager, db, jwt);
    }

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"twf-auth-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task RegisterAsync_Should_Create_User_And_Return_Tokens_When_Data_Valid()
    {
        await using var db = CreateDbContext();

        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { AuthConstants.WorkerRole });

        var service = CreateAuthService(userManager.Object, db);

        var request = new RegisterRequest
        {
            UserName = "new.user",
            Email = "new.user@test.local",
            Password = "P@ssw0rd!",
            FullName = "New User"
        };

        var result = await service.RegisterAsync(request);

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        (await db.RefreshTokens.CountAsync()).Should().Be(1);

        userManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u => u.UserName == "new.user"), "P@ssw0rd!"), Times.Once);
        userManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), AuthConstants.WorkerRole), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_Null_When_Username_Exists()
    {
        await using var db = CreateDbContext();

        var existing = new ApplicationUser { Id = Guid.NewGuid(), UserName = "exist", Email = "exist@test.local" };
        var userManager = CreateUserManagerMock();
        userManager.Setup(x => x.FindByNameAsync("exist")).ReturnsAsync(existing);

        var service = CreateAuthService(userManager.Object, db);

        var request = new RegisterRequest
        {
            UserName = "exist",
            Email = "exist@test.local",
            Password = "P@ssw0rd!",
            FullName = "Exist User"
        };

        var result = await service.RegisterAsync(request);

        result.Should().BeNull();
    }
}
