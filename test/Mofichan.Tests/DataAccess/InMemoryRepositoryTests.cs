using System.Linq;
using Mofichan.Core;
using Mofichan.DataAccess.Database;
using Mofichan.Tests.TestUtility;
using Shouldly;
using Xunit;

namespace Mofichan.Tests.DataAccess
{
    public class InMemoryRepositoryTests
    {
        [Fact]
        public void In_Memory_Repository_Should_Recall_Stored_Objects()
        {
            // GIVEN an in memory repository.
            var repository = new InMemoryRepository();

            // GIVEN set of objects to store.
            var numbers = new[] { 1, 42, 103 }.ToList();

            var words = new[] { "foo", "bar", "baz" }.ToList();

            var messages = new[]
            {
                new MessageContext(new MockUser("Tom", "Tom"), new MockUser("Jerry", "Jerry"), "hello"),
                new MessageContext(new MockUser("Amy", "Amy"), new MockUser("Betty", "Betty"), "How are you?"),
            }.ToList();

            // EXPECT we can retrieve the set of numbers after storing them.
            numbers.ForEach(it => repository.Add(it));

            repository.All<int>().ShouldBe(numbers, ignoreOrder: true);
            repository.All<string>().ShouldBeEmpty();
            repository.All<MessageContext>().ShouldBeEmpty();

            // EXPECT we can retrieve the set of words after storing them.
            words.ForEach(it => repository.Add(it));

            repository.All<int>().ShouldBe(numbers, ignoreOrder: true);
            repository.All<string>().ShouldBe(words, ignoreOrder: true);
            repository.All<MessageContext>().ShouldBeEmpty();

            // EXPECT we can retrieve the set of messages after storing them.
            messages.ForEach(it => repository.Add(it));

            repository.All<int>().ShouldBe(numbers, ignoreOrder: true);
            repository.All<string>().ShouldBe(words, ignoreOrder: true);
            repository.All<MessageContext>().ShouldBe(messages, ignoreOrder: true);
        }
    }
}
