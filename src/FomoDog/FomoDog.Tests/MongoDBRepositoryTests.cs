using FluentAssertions;
using FomoDog.Context.Models;
using FomoDog.Context.MongoDB;
using FomoDog.Context.MongoDB.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace FomoDog.Tests
{
    public class MongoDBRepositoryTests
    {
        private readonly Mock<IMongoCollection<Activity>> _mockMongoCollection;
        private readonly MongoDBRepository _repository;
        private const string TestDbConnectionString = "mongodb://localhost:27017";
        private const string TestDbName = "TestDb";
        private const string ActivityCollectionName = "activities";

        public MongoDBRepositoryTests()
        {
            var _mockMongoClient = new Mock<IMongoClient>();
            var _mockMongoDatabase = new Mock<IMongoDatabase>();
            _mockMongoCollection = new Mock<IMongoCollection<Activity>>();

            var options = Options.Create(new MongoDBOptions
            {
                ConnectionString = TestDbConnectionString,
                Database = TestDbName
            });

            _mockMongoClient
                .Setup(client => client.GetDatabase(TestDbName, null))
                .Returns(_mockMongoDatabase.Object);

            _mockMongoDatabase
                .Setup(db => db.GetCollection<Activity>(ActivityCollectionName, null))
                .Returns(_mockMongoCollection.Object);

            _repository = new MongoDBRepository(_mockMongoClient.Object, options);
        }

        [Fact]
        public async Task AddActivity_ShouldInvokeInsertOneAsync_Once()
        {
            // Arrange
            var chatActivity = new ChatActivity
            {
                ChatId = "test",
                From = "user",
                Content = "Test message content",
                Date = DateTime.UtcNow,
                RawMessage = "{\"name\":\"test\"}"
            };

            // Act
            await _repository.AddActivity(chatActivity);

            // Assert
            _mockMongoCollection.Verify(
                collection => collection.InsertOneAsync(
                    It.Is<Activity>(activity =>
                        activity.ChatId == chatActivity.ChatId &&
                        activity.From == chatActivity.From &&
                        activity.Content == chatActivity.Content),
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetAllActivity_ShouldReturnListOfChatActivity()
        {
            // Arrange
            string chatId = "someChatId";
            var activityList = CreateTestActivities(chatId);

            SetupMockedAsyncCursor(activityList);

            // Act
            var result = await _repository.GetAllActivity(chatId);

            // Assert
            var expectedChatActivityList = MapToChatActivityList(activityList);
            result.Should().BeEquivalentTo(expectedChatActivityList, options => options.ComparingByMembers<ChatActivity>());
        }

        private List<Activity> CreateTestActivities(string chatId)
        {
            return new List<Activity>
            {
                new Activity { Id = ObjectId.GenerateNewId(), ChatId = chatId, Content = "Test content 1", Date = DateTime.UtcNow.AddHours(-1) },
                new Activity { Id = ObjectId.GenerateNewId(), ChatId = chatId, Content = "Test content 2", Date = DateTime.UtcNow.AddHours(-2) },
                // Additional test activities can be added here
            };
        }

        private void SetupMockedAsyncCursor(List<Activity> activityList)
        {
            var asyncCursorMock = new Mock<IAsyncCursor<Activity>>();
            asyncCursorMock.Setup(_ => _.Current).Returns(activityList);
            asyncCursorMock
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            _mockMongoCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Activity>>(),
                    It.IsAny<FindOptions<Activity, Activity>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(asyncCursorMock.Object));
        }

        private List<ChatActivity> MapToChatActivityList(List<Activity> activities)
        {
            // This mapping assumes certain logic. Adjust according to your actual mapping logic.
            return activities.ConvertAll(activity => new ChatActivity
            {
                ChatId = activity.ChatId,
                From = activity.From, // Or whatever logic is used to derive 'From'
                Content = activity.Content,
                Date = activity.Date,
                // RawMessage might need special handling if it's not a direct map
            });
        }
    }
}
