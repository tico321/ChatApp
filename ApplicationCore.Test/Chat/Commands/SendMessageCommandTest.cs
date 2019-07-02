using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Chat.Commands;
using ApplicationCore.Chat.Domain;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Users.Domain;
using FakeItEasy;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApplicationCore.Test.Chat.Commands
{
    public class SendMessageCommandTest
    {
        [Fact]
        public async Task Handler_WhenTheUserDoesntExistsThrowsUnAuthorizedException()
        {
            try
            {
                var (context, botService, sut) = GetSut();
                var principal = A.Fake<ClaimsPrincipal>();
                A.CallTo(() => principal.Claims).Returns(new List<Claim>{ new Claim(ClaimTypes.Name, "unknown@gmail.com") });
                var command = new SendMessageCommand
                {
                    Content = "",
                    User = principal
                };

                var result = await sut.Handle(command, new CancellationToken());
                Assert.False(true, "should throw exception");
            }
            catch(UnAuthorizedException e)
            {
                Assert.NotNull(e);
            }
        }

        [Fact]
        public async Task Handler_WhenIsNotACommandStoresTheMessage()
        {
            var (context, botService, sut) = GetSut();
            var user = new ApplicationUser { Email = "email@gmail.com", UserName = "email@gmail.com" };
            context.Users.Add(user);
            context.SaveChanges();
            var principal = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => principal.Claims).Returns(new List<Claim>{ new Claim(ClaimTypes.Name, user.UserName) });
            var command = new SendMessageCommand
            {
                Content = "msg",
                User = principal
            };

            var result = await sut.Handle(command, new CancellationToken());

            var actual = context.Messages.Find(result.Id);
            Assert.NotNull(actual);
            Assert.Equal(command.Content, actual.Content);
        }

        [Fact]
        public async Task Handler_WhenIsACommandSendsRequestToBot()
        {
            var (context, botService, sut) = GetSut();
            var user = new ApplicationUser { Email = "email@gmail.com", UserName = "email@gmail.com" };
            context.Users.Add(user);
            context.SaveChanges();
            var principal = A.Fake<ClaimsPrincipal>();
            A.CallTo(() => principal.Claims).Returns(new List<Claim>{ new Claim(ClaimTypes.Name, user.UserName) });
            var command = new SendMessageCommand
            {
                Content = "/stock=appl.us",
                User = principal
            };

            var result = await sut.Handle(command, new CancellationToken());

            var actual = context.Messages.Find(result.Id);
            Assert.Null(actual);
            Assert.Equal(0, context.Messages.Count());
            A
                .CallTo(() => botService.SendCommand(
                    A<CommandMessage>.That.Matches(cm => cm.Type == MessageCommandType.Stock && cm.Command == "appl.us"),
                    A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Handler_WhenTheCommandIsNotRecognizedThrowsBadRequestException()
        {
            try
            {
                var (context, botService, sut) = GetSut();
                var user = new ApplicationUser { Email = "email@gmail.com", UserName = "email@gmail.com" };
                context.Users.Add(user);
                context.SaveChanges();
                var principal = A.Fake<ClaimsPrincipal>();
                A.CallTo(() => principal.Claims).Returns(new List<Claim>{ new Claim(ClaimTypes.Name, user.UserName) });
                var command = new SendMessageCommand
                {
                    Content = "/unknownCommand=appl.us",
                    User = principal
                };

                var result = await sut.Handle(command, new CancellationToken());
            }
            catch(BadRequestException e)
            {
                var errors = e.Errors;
                Assert.Single(errors);
                var err = errors.First();
                Assert.Equal("CommandNotRecognized", err.Item1);
                Assert.Equal("The command unknownCommand is not valid", err.Item2);
            }
        }

        [Fact]
        public async Task Handler_WhenTheCommandIsMissingAnArgument()
        {
            try
            {
                var (context, botService, sut) = GetSut();
                var user = new ApplicationUser { Email = "email@gmail.com", UserName = "email@gmail.com" };
                context.Users.Add(user);
                context.SaveChanges();
                var principal = A.Fake<ClaimsPrincipal>();
                A.CallTo(() => principal.Claims).Returns(new List<Claim>{ new Claim(ClaimTypes.Name, user.UserName) });
                var command = new SendMessageCommand
                {
                    Content = "/unknownCommand=",
                    User = principal
                };

                var result = await sut.Handle(command, new CancellationToken());
            }
            catch(BadRequestException e)
            {
                var errors = e.Errors;
                Assert.Single(errors);
                var err = errors.First();
                Assert.Equal("CommandArgumentsMissing", err.Item1);
                Assert.Equal("Arguments are missing for the command", err.Item2);
            }
        }

        [Theory]
        [InlineData("content", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("MoreThan100 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890 1234567890", false)]
        public void Validator(string content, bool isValid)
        {
            var sut = new SendMessageCommandValidator();

            var actual = sut.Validate(new SendMessageCommand { Content = content });

            Assert.True(isValid == actual.IsValid);
        }

        public (ApplicationDbContext, IBotService, SendMessageCommandHandler) GetSut()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());
            var dbContext = new ApplicationDbContext(builder.Options);
            var botService = A.Fake<IBotService>();
            return (dbContext, botService, new SendMessageCommandHandler(dbContext, botService));
        }
    }
}