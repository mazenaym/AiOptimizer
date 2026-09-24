//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using Moq;
//using PromptOptimizer.Application.Common.Interfaces;
//using PromptOptimizer.Application.Optimization.Models;
//using PromptOptimizer.Application.Optimization.Services;
//using PromptOptimizer.Application.Prompts.Commands.OptimizePrompt;
//using PromptOptimizer.Domain.Entities;
//using PromptOptimizer.Domain.Enums;
//using PromptOptimizer.Domain.ValueObjects;
//using PromptOptimizer.Infrastructure.Persistence;
//using Xunit;

//namespace PromptOptimizer.UnitTests;

//public class OptimizePromptCommandHandlerTests
//{
//    [Fact]
//    public async Task Handle_ShouldReturnSuccess_WhenRequestIsValid()
//    {
//        // Arrange
//        var options = new DbContextOptionsBuilder<AppDbContext>()
//            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
//            .Options;

//        using var context = new AppDbContext(options);

//        var userId = Guid.NewGuid();
//        context.Users.Add(new User
//        {
//            Id = userId,
//            Email = "test@example.com",
//            FullName = "Test User"
//        });
//        await context.SaveChangesAsync();

//        var currentUserServiceMock = new Mock<ICurrentUserService>();
//        currentUserServiceMock.Setup(s => s.UserId).Returns(userId);

//        var optimizerEngineMock = new Mock<IPromptOptimizerEngine>();
//        optimizerEngineMock.Setup(e => e.OptimizeAsync(It.IsAny<PromptOptimizationRequest>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(new AIOptimizationResponse(
//                "Optimized test prompt content",
//                "Explanation here",
//                new TokenUsage(10, 20),
//                150.0
//            ));

//        var handler = new OptimizePromptCommandHandler(context, optimizerEngineMock.Object, currentUserServiceMock.Object);

//        var command = new OptimizePromptCommand(
//            PromptId: null,
//            Content: "Write a poem about space",
//            Category: PromptCategory.CreativeWriting,
//            Provider: "Gemini",
//            ModelKey: "gemini-1.5-flash",
//            CustomInstructions: "Make it vivid"
//        );

//        // Act
//        var result = await handler.Handle(command, CancellationToken.None);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        result.Value.Should().NotBeNull();
//        result.Value!.OptimizedContent.Should().Be("Optimized test prompt content");
//        result.Value.PromptTokens.Should().Be(10);
//        result.Value.CompletionTokens.Should().Be(20);
//    }
//}
