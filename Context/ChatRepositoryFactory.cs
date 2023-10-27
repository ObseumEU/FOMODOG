using FomoDog.Context.FileRepository;
using FomoDog.Context.MongoDB;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
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

        public ChatRepositoryFactory(IFeatureManager featureManager, IFileSystem fileSystem, IOptions<FileRepositoryOption> chatRepositoryOption, IOptions<MongoDBOptions> mongoDBOptions)
        {
            _featureManager = featureManager;
            _fileSystem = fileSystem;
            _chatRepositoryOption = chatRepositoryOption;
            _mongoDBOptions = mongoDBOptions;
        }

        public async Task<IChatRepository> CreateRepositoryAsync()
        {
            if (await _featureManager.IsEnabledAsync(FeatureFlags.STORE_DATA_IN_DATABASE))
            {
                return new MongoDBRepository(_mongoDBOptions);
            }
            else
            {
                return new FileRepository.FileRepository(_fileSystem, _chatRepositoryOption);
            }
        }
    }
}
