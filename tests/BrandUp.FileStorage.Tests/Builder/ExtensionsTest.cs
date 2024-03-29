﻿//using BrandUp.FileStorage.Abstract;
//using BrandUp.FileStorage.AwsS3;
//using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.Abstract;
using BrandUp.FileStorage.AwsS3;
using BrandUp.FileStorage.AwsS3.Configuration;
using BrandUp.FileStorage.FileSystem;
using BrandUp.FileStorage.Tests.Storages.Aws;
using BrandUp.FileStorage.Tests.Storages.Aws.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage.Builder
{
    public class ExtensionsTest : FileStorageTestBase<FakeFile>
    {
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddAwsS3Storage(new Dictionary<string, AwsS3Configuration>()
            {
                {
                    "Default" ,
                    new()
                    {
                        ServiceUrl = "https://s3.yandexcloud.net",
                        AuthenticationRegion = "ru-central1",
                        AccessKeyId = "adadass",
                        SecretAccessKey = "adadass"
                    }
                },
                {
                    "FakeAwsFile" ,
                    new()
                    {
                        BucketName = "bucket"
                    }
                },
            })
            .AddAwsS3Bucket<FakeFile>("FakeAwsFile")
            .AddAwsS3Bucket<AttributedFakeFile>((options) =>
            {
                options.BucketName = "bucket2";
            }, "FakeAwsFile2");

            builder.AddFolderStorage((o) => { })
            .AddFolderFor<Storages.FileSystem.FakeFile>((o) =>
            {
                o.MetadataPath = "D:\\Test";
                o.ContentPath = "D:\\Test\\Metadata";
            }, "FakeLocalFile");

            base.OnConfigure(services, builder);
        }

        [Fact]
        public void Success()
        {
            var awsFakeStorage = Services.GetService<IFileStorage<FakeFile>>();
            Assert.NotNull(awsFakeStorage);
            var awsFakeStorage2 = Services.GetService<IFileStorage<AttributedFakeFile>>();
            Assert.NotNull(awsFakeStorage2);
            var localFakeStorage = Services.GetService<IFileStorage<Storages.FileSystem.FakeFile>>();
            Assert.NotNull(localFakeStorage);
        }

        internal override FakeFile CreateMetadataValue()
        {
            throw new NotImplementedException();
        }
    }
}
