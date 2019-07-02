using System;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Chat.Commands;
using ApplicationCore.Exceptions;
using ApplicationCore.Interfaces;
using ApplicationCore.Users.Domain;
using FakeItEasy;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApplicationCore.Test.Chat.Commands
{
    public class HandleQueuedMessageCommandTest
    {
        [Fact]
        public async Task Handler_WhenTheUserIsNotFoundThrowsUnAuthorizedException()
        {
            var (context, broadcastService, sut) = this.GetSut();
            var command = new HandleQueuedMessageCommand
            {
                Content = "message",
                UserEmail = "unknown@gmail.com"
            };

            try
            {
                var actual = await sut.Handle(command, new CancellationToken());
                Assert.True(false, "Should throw exception");
            }
            catch(UnAuthorizedException e)
            {
                Assert.NotNull(e);
            }
        }

        [Fact]
        public async Task Handler_StoresTheMessageAndNotifiesTheClientThereIsANewMessage()
        {
            var (context, broadcastService, sut) = this.GetSut();
            var user = new ApplicationUser { Email = "email@gmail.com", UserName = "email@gmail.com" };
            context.Users.Add(user);
            context.SaveChanges();
            var command = new HandleQueuedMessageCommand
            {
                Content = "message",
                UserEmail = user.Email
            };

            var actual = await sut.Handle(command, new CancellationToken());

            var msg = context.Messages.Find(actual.Id);
            Assert.NotNull(msg);
            Assert.Equal(command.Content, msg.Content);
            A.CallTo(() => broadcastService.NotifyNewMessage()).MustHaveHappened();
        }

        [Theory]
        [InlineData("content", "user@gmail.com", true)]
        [InlineData("content", "user", false)]
        [InlineData("content", "", false)]
        [InlineData("content", null, false)]
        [InlineData(null, "user@gmail.com", false)]
        [InlineData("", "user@gmail.com", false)]
        public void Validator(string content, string email, bool isValid)
        {
            var sut = new HandleQueuedMessageCommandValidator();
            var command = new HandleQueuedMessageCommand { Content = content, UserEmail = email };

            var actual = sut.Validate(command);

            Assert.True(isValid == actual.IsValid);
        }

        public (ApplicationDbContext, IBroadcastService, HandleQueuedMessageCommandHandler) GetSut()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());
            var dbContext = new ApplicationDbContext(builder.Options);
            var broadcastService = A.Fake<IBroadcastService>();
            return (dbContext, broadcastService, new HandleQueuedMessageCommandHandler(dbContext, broadcastService));
        }
    }
}