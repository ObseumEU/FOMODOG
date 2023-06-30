
using FomoDog.Context;
using FomoDog.Context.Models;
using Microsoft.Extensions.Options;
using System.IO.Abstractions.TestingHelpers;
using Xunit;

namespace FomoDog.Tests
{
    public class ChatRepositoryTests
    {
        private ChatActivity CreateChatActivity(string chatId, string from, string content, DateTime date)
        {
            return new ChatActivity
            {
                ChatId = chatId,
                From = from,
                Content = content,
                Date = date,
                RawMessage = string.Empty,
            };
        }

        [Fact]
        public async Task Shoud_Return_All_Messages()
        {
            MockFileSystem fileSystem;
            IOptions<ChatRepositoryOption> moqOptions;
            ArrageDependencies(out fileSystem, out moqOptions);

            var activity = CreateChatActivity("1", "Olda Master", "Hello", DateTime.UtcNow);

            Context.IChatRepository repository = new ChatRepository(fileSystem.FileSystem, moqOptions);
            repository.AddActivity(activity);
            var activities = await repository.GetAllActivity("1");

            Assert.Single(activities);
            Assert.Equal(activity.ChatId, activities.First().ChatId);
            Assert.Equal(activity.From, activities.First().From);
        }

        private static void ArrageDependencies(out MockFileSystem fileSystem, out IOptions<ChatRepositoryOption> moqOptions)
        {
            fileSystem = new MockFileSystem();
            moqOptions = Options.Create(new ChatRepositoryOption()
            {
                RepositoryPath = "./data.txt",
                MaxMessagesStoreCount = 30
            });
        }

        [Fact]
        public async Task Shoud_Trim_Activities()
        {
            MockFileSystem fileSystem;
            IOptions<ChatRepositoryOption> moqOptions;
            ArrageDependencies(out fileSystem, out moqOptions);
            Context.IChatRepository repository = new ChatRepository(fileSystem.FileSystem, moqOptions);
            ChatActivity checkActivity = null;
            for (int i = 0; i < 30; i++)
            {
                var newActivity = CreateChatActivity("1", Guid.NewGuid().ToString(), $"{i}", DateTime.UtcNow.AddSeconds(+i));
                await repository.AddActivity(newActivity);

                if (i == 20)
                {
                    checkActivity = newActivity;
                }
            }

            var count = (await repository.GetAllActivity("1")).Count;
            Assert.Equal(30, count);
            await repository.TrimActivity("1", 10);
            count = (await repository.GetAllActivity("1")).Count;
            Assert.Equal(10, count);
            var all = (await repository.GetAllActivity("1"));
            Assert.Equal(checkActivity.Content, all[0].Content);
            Assert.Equal("1", (await repository.GetAllActivity("1")).First().ChatId);
        }
    }
}