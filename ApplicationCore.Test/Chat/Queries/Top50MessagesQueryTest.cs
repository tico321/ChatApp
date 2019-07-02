using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Chat.Domain;
using ApplicationCore.Chat.Queries;
using ApplicationCore.Users.Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApplicationCore.Test.Chat.Queries
{
    public class Top50MessagesQueryTest
    {
        [Fact]
        public async Task Handle_ReturnsMessagesSortedByDate()
        {
            var (dbContext, sut) = GetSut();
            var user = new ApplicationUser { Email = "email@gmail.com", UserName = "email@gmail.com" };
            var older = new Message("older", user);
            dbContext.Messages.Add(older);
            var newer = new Message("newer", user);
            dbContext.Messages.Add(newer);
            dbContext.SaveChanges();

            var result = await sut.Handle(new Top50MessagesQuery(), new CancellationToken());

            Assert.Equal(2, result.Messages.Count());
            var first = result.Messages.First();
            Assert.Equal(newer.Content, first.Content);
        }

        [Fact]
        public async Task Handle_NeverReturnsMoreThan50Messages()
        {
            var (dbContext, sut) = GetSut();
            var user = new ApplicationUser { Email = "email@gmail.com", UserName = "email@gmail.com" };
            Enumerable.Range(0, 60)
                .Select(i => new Message($"msg{i}", user))
                .ToList()
                .ForEach(m => dbContext.Messages.Add(m));
            dbContext.SaveChanges();

            var result = await sut.Handle(new Top50MessagesQuery(), new CancellationToken());

            Assert.Equal(50, result.Messages.Count());
        }

        public (ApplicationDbContext, Top50MessagesQueryHandler) GetSut()
        {
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString());
            var dbContext = new ApplicationDbContext(builder.Options);
            return (dbContext, new Top50MessagesQueryHandler(dbContext));
        }
    }
}