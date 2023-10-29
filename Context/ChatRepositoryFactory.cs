using FomoDog.Context.FileRepository;
using FomoDog.Context.MongoDB;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using MongoDB.Driver;
using System.IO.Abstractions;

namespace FomoDog.Context
{
    public interface IChatRepositoryFactory
    {
        Task<IChatRepository> CreateRepositoryAsync();
    }

    public class ChatRepositoryFactory : IChatRepositoryFactory
    {
        private readonly IFeatureManager _featureManager;
        private readonly IFileSystem _fileSystem;
        private readonly IOptions<FileRepositoryOption> _chatRepositoryOption;
        private readonly IOptions<MongoDBOptions> _mongoDBOptions;
        private readonly IMongoClient _mongoClient;

        public ChatRepositoryFactory(IFeatureManager featureManager, IFileSystem fileSystem, IOptions<FileRepositoryOption> chatRepositoryOption, IOptions<MongoDBOptions> mongoDBOptions, IMongoClient mongoClient)
        {
            _featureManager = featureManager;
            _fileSystem = fileSystem;
            _chatRepositoryOption = chatRepositoryOption;
            _mongoDBOptions = mongoDBOptions;
            _mongoClient = mongoClient;
        }

        public async Task<IChatRepository> CreateRepositoryAsync()
        {
            if (await _featureManager.IsEnabledAsync(FeatureFlags.STORE_DATA_IN_DATABASE))
            {
                return new MongoDBRepository(_mongoClient, _mongoDBOptions);
            }
            else
            {
                return new FileRepository.FileRepository(_fileSystem, _chatRepositoryOption);
            }
        }
    }
}
