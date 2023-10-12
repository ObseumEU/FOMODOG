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
        private readonly IOptions<ChatRepositoryOption> _options;

        public ChatRepositoryFactory(IFeatureManager featureManager, IFileSystem fileSystem, IOptions<ChatRepositoryOption> options)
        {
            _featureManager = featureManager;
            _fileSystem = fileSystem;
            _options = options;
        }

        public async Task<IChatRepository> CreateRepositoryAsync()
        {
            if (await _featureManager.IsEnabledAsync(FeatureFlags.STORE_DATA_IN_DATABASE))
            {
                throw new Exception("STORE_DATA_IN_DATABASE Not implemented");
            }
            else
            {
                return new ChatRepository(_fileSystem, _options);
            }
        }
    }
}
