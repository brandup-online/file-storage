﻿using BrandUp.FileStorage.Attributes;
using BrandUp.FileStorage.Builder;
using BrandUp.FileStorage.Exceptions;
using BrandUp.FileStorage.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.FileStorage
{
    public class FileStorageTests : FileStorageTestBase
    {
        readonly TestFileContext testFileContext;
        readonly AttributedsTestFileContext attributedTestFileContext;

        #region FileStorageTestBase members
        protected override void OnConfigure(IServiceCollection services, IFileStorageBuilder builder)
        {
            builder.AddTestProvider("test", options =>
            {
                options.MaxFilesByBucket = 10;
                options.MaxFileSize = 100;
            });

            services
                .AddFileContext<TestFileContext>("test")
                .AddFileContext<AttributedsTestFileContext>("test");
        }
        #endregion

        public FileStorageTests()
        {
            testFileContext = Services.GetRequiredService<TestFileContext>();
            attributedTestFileContext = Services.GetRequiredService<AttributedsTestFileContext>();
        }

        #region Tests 

        /// <summary>
        /// Test cheks correct work of storage builder, and context initializer.
        /// </summary>
        [Fact]

        public void Init_Success()
        {
            Assert.NotNull(testFileContext);
            Assert.NotNull(testFileContext.StorageProvider);

            var tempFiles = testFileContext.FileStorageTestFiles;
            Assert.NotNull(tempFiles);
        }

        /// <summary>
        /// CRUD operations for storage.
        /// </summary>
        [Fact]
        public async void CRUD_Success()
        {
            #region Preparation 

            var collection = testFileContext.FileStorageTestFiles;

            TestFile file = new()
            {
                FileName = "Test",
                Size = 100,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.Date,
            };

            using MemoryStream stream = new(image);

            #endregion

            var fileinfo = await TestUploadAsync(collection, file, stream);
            EqualsAssert(file, fileinfo.Metadata);

            var getFileinfo = await TestGetAsync(collection, fileinfo.Id);
            EqualsAssert(file, getFileinfo.Metadata);

            await TestReadAsync(collection, fileinfo.Id, stream);

            await TestDeleteAsync(collection, fileinfo.Id);
        }

        /// <summary>
        /// Test check correct work of <see cref="MetadataIgnoreAttribute"/> 
        /// </summary>
        [Fact]
        public async Task Attributes_Ignore()
        {
            var collection = attributedTestFileContext.AttributedTestFiles;
            AttributedTestFile file = new()
            {
                FileName = "Test",
                Size = 100,
                MailingId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.Date,
                Ignore = "232"
            };

            using MemoryStream stream = new(image);

            var uploaded = await TestUploadAsync(collection, file, stream);
            var fileinfo = await TestGetAsync(collection, uploaded.Id);
            Assert.Null(fileinfo.Metadata.Ignore);
        }


        /// <summary>
        /// Test check correct work of <see cref="MetadataRequiredAttribute"/> 
        /// </summary>
        [Fact]
        public async Task Attributes_Required()
        {
            #region Preparation

            var collection = attributedTestFileContext.AttributedTestFiles;
            AttributedTestFile file = new()
            {
                FileName = null,
                Size = 100,
                MailingId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow.Date,
                Ignore = "232"
            };

            using MemoryStream stream = new(image);

            #endregion

            await Assert.ThrowsAsync<PropertyRequiredException>(async () =>
            {
                var fileinfo = await TestUploadAsync(collection, file, stream);
                Assert.Null(fileinfo.Metadata.Ignore);
            });
        }

        #endregion


    }
}
