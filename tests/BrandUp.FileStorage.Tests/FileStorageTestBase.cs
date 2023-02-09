﻿using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.Folder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Tests
{
    public abstract class FileStorageTestBase : IAsyncLifetime
    {
        readonly ServiceProvider rootServiceProvider;
        readonly IServiceScope serviceScope;

        public IServiceProvider RootServices => rootServiceProvider;
        public IServiceProvider Services => serviceScope.ServiceProvider;

        public FileStorageTestBase()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            var config = new ConfigurationBuilder()
               .AddUserSecrets(typeof(FileStorageTestBase).Assembly)
               .AddJsonFile("appsettings.test.json", true)
               .AddEnvironmentVariables()
               .Build();

            var builder = services.AddFileStorage();

            builder.AddAwsS3Storage(config.GetSection("TestCloudStorage"))
                    .AddAwsS3Bucket<_fakes.Aws.FakeAwsFile>()
                    .AddAwsS3Bucket<_fakes.Aws.AttributedFakeFile>("FakeAwsFile")
                    .AddAwsS3Bucket<_fakes.Aws.FakeMetadataOld>("FakeAwsFile")
                    .AddAwsS3Bucket<_fakes.Aws.FakeMetadataNew>("FakeAwsFile");

            builder.AddFolderStorage(config.GetSection("TestFolderStorage"))
                  .AddFolderFor<_fakes.Local.FakeLocalFile>();

            OnConfigure(services);

            rootServiceProvider = services.BuildServiceProvider();
            serviceScope = rootServiceProvider.CreateScope();
        }

        #region IAsyncLifetime region

        public async Task InitializeAsync()
        {
            await OnInitializeAsync(rootServiceProvider, serviceScope.ServiceProvider);
        }

        public async Task DisposeAsync()
        {
            await OnFinishAsync(rootServiceProvider, serviceScope.ServiceProvider);

            serviceScope.Dispose();
            await rootServiceProvider.DisposeAsync();
        }

        #endregion

        //protected async Task DoCRUD<T>(IFileStorage<T> client, T metadata, Stream stream) where T : class, IFileMetadata, new()
        //{
        //    var fileinfo = await client.UploadFileAsync(metadata, stream, CancellationToken.None);
        //    Assert.NotNull(fileinfo);

        //    var getFileinfo = await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None);
        //    Assert.NotNull(getFileinfo);

        //    var inputMetadata = metadata as FakeFile;
        //    var downloadedMetadata = getFileinfo.Metadata as FakeFile;

        //    Assert.Equal(inputMetadata.FileName, downloadedMetadata.FileName);
        //    Assert.Equal(inputMetadata.Extension, downloadedMetadata.Extension);
        //    Assert.Equal(inputMetadata.FakeInt, downloadedMetadata.FakeInt);
        //    Assert.Equal(inputMetadata.FakeTimeSpan, downloadedMetadata.FakeTimeSpan);
        //    Assert.Equal(inputMetadata.FakeInner.FakeGuid, downloadedMetadata.FakeInner.FakeGuid);
        //    Assert.Equal(inputMetadata.FakeInner.FakeBool, downloadedMetadata.FakeInner.FakeBool);
        //    Assert.Equal(inputMetadata.FakeDateTime, downloadedMetadata.FakeDateTime);

        //    Assert.Equal(stream.Length, getFileinfo.Size);

        //    using var downlodadedStream = await client.ReadFileAsync(fileinfo.FileId, CancellationToken.None);
        //    Assert.NotNull(downlodadedStream);
        //    Assert.Equal(stream.Length, downlodadedStream.Length);

        //    var isDeleted = await client.DeleteFileAsync(fileinfo.FileId, CancellationToken.None);
        //    Assert.True(isDeleted);

        //    Assert.Null(await client.GetFileInfoAsync(fileinfo.FileId, CancellationToken.None));
        //}

        #region Virtual members

        protected virtual void OnConfigure(IServiceCollection services) { }
        protected virtual Task OnInitializeAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;
        protected virtual Task OnFinishAsync(IServiceProvider rootServices, IServiceProvider scopeServices) => Task.CompletedTask;

        #endregion
    }
}
