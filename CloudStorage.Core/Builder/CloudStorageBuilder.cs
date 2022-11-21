﻿using Microsoft.Extensions.DependencyInjection;

namespace CloudStorage.Extensions
{
    public class CloudStorageBuilder : ICloudStorageBuilder
    {
        public IServiceCollection Services { get; set; }

        public CloudStorageBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }

    public interface ICloudStorageBuilder
    {
        public IServiceCollection Services { get; set; }
    }
}
