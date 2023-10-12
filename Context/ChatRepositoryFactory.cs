using FomoDog.Context.Database;
using FomoDog.Context.FileRepository;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private readonly IOptions<DatabaseRepositoryOptions> _databaseRepositoryOptions;

        public ChatRepositoryFactory(IFeatureManager featureManager, IFileSystem fileSystem, IOptions<FileRepositoryOption> chatRepositoryOption, IOptions<DatabaseRepositoryOptions> databaseRepositoryOptions)
        {
            _featureManager = featureManager;
            _fileSystem = fileSystem;
            _chatRepositoryOption = chatRepositoryOption;
            _databaseRepositoryOptions = databaseRepositoryOptions;
        }

        public async Task<IChatRepository> CreateRepositoryAsync()
        {
            if (await _featureManager.IsEnabledAsync(FeatureFlags.STORE_DATA_IN_DATABASE))
            {
                return new DatabaseRepository(_databaseRepositoryOptions);
            }
            else
            {
                return new FileRepository.FileRepository(_fileSystem, _chatRepositoryOption);
            }
        }
    }
}
