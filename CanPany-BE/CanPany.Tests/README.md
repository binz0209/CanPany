# CanPany Tests

Test suite cho CanPany API với Unit Tests và Integration Tests.

## Cấu trúc

```
CanPany.Tests/
├── Common/
│   ├── TestDatabase.cs              # Helper cho test MongoDB database
│   └── TestWebApplicationFactory.cs  # WebApplicationFactory cho integration tests
├── ApplicationTests/
│   ├── UserServiceTests.cs          # Unit tests cho UserService
│   └── AuthServiceTests.cs          # Unit tests cho AuthService
├── InfrastructureTests/
│   └── UserRepositoryTests.cs       # Integration tests cho UserRepository với MongoDB
├── ApiTests/
│   ├── AuthControllerTests.cs       # Integration tests cho AuthController
│   ├── HealthControllerTests.cs     # Integration tests cho HealthController
│   └── ProjectsControllerTests.cs  # Integration tests cho ProjectsController
└── IntegrationTests/
    └── FullFlowTests.cs             # End-to-end integration tests
```

## Chạy Tests

### Chạy tất cả tests
```bash
dotnet test
```

### Chạy tests với coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Chạy tests theo category
```bash
# Chỉ unit tests
dotnet test --filter "FullyQualifiedName~ApplicationTests"

# Chỉ integration tests
dotnet test --filter "FullyQualifiedName~ApiTests"
```

## Yêu cầu

- MongoDB chạy trên `localhost:27017` (cho integration tests)
- .NET 8.0 SDK

## Test Types

### Unit Tests
- **ApplicationTests**: Test các Application Services với mocked dependencies
- Sử dụng **Moq** để mock repositories và services
- Sử dụng **FluentAssertions** cho assertions dễ đọc

### Integration Tests
- **InfrastructureTests**: Test repositories với MongoDB thật
- **ApiTests**: Test API controllers với WebApplicationFactory
- **IntegrationTests**: Test full flow end-to-end

## Test Database

Integration tests sử dụng test database riêng:
- Database name: `CanPany_Test_{Guid}`
- Tự động xóa sau khi test xong
- Isolated cho mỗi test class

## Examples

### Unit Test Example
```csharp
[Fact]
public async Task GetByIdAsync_ShouldReturnUser_WhenUserExists()
{
    // Arrange
    var userId = "user123";
    var expectedUser = new User { Id = userId, Email = "test@example.com" };
    _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(expectedUser);

    // Act
    var result = await _userService.GetByIdAsync(userId);

    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(userId);
}
```

### Integration Test Example
```csharp
[Fact]
public async Task Register_ShouldReturnToken_WhenValidRequest()
{
    // Arrange
    var request = new { FullName = "Test", Email = "test@example.com", Password = "Password123!" };

    // Act
    var response = await _client.PostAsJsonAsync("/api/auth/register", request);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Notes

- Integration tests cần MongoDB chạy
- Test database được tạo và xóa tự động
- External services (Email, SignalR) được mock trong tests
- JWT tokens được generate với test key

