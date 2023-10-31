using FomoDog.Context.FileRepository;
using FomoDog.Context.Models;
using Microsoft.Extensions.Options;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace FomoDog.Tests
{
    public class ChatFileRepositoryTests
    {
        private ChatActivity CreateChatActivity(string chatId, string from, string content, DateTime date)
        {
            return new ChatActivity
            {
                ChatId = chatId,
                From = from,
                Content = content,
                Date = date,
                RawMessage = "{ \"Test\":\"Test\"}",
            };
        }

        [Fact]
        public async Task Shoud_Return_All_Messages()
        {
            MockFileSystem fileSystem;
            IOptions<FileRepositoryOption> moqOptions;
            ArrageDependencies(out fileSystem, out moqOptions);

            var activity = CreateChatActivity("1", "Olda Master", "Hello", DateTime.UtcNow);

            Context.IChatRepository repository = new FileRepository(fileSystem.FileSystem, moqOptions);
            await repository.AddActivity(activity);
            var activities = await repository.GetAllActivity("1");

            Assert.Single(activities);
            Assert.Equal(activity.ChatId, activities.First().ChatId);
            Assert.Equal(activity.From, activities.First().From);
        }

        private static void ArrageDependencies(out MockFileSystem fileSystem, out IOptions<FileRepositoryOption> moqOptions)
        {
            fileSystem = new MockFileSystem();
            moqOptions = Options.Create(new FileRepositoryOption()
            {
                RepositoryPath = "./data.txt",
                MaxMessagesStoreCount = 30
            });
        }
    }
}