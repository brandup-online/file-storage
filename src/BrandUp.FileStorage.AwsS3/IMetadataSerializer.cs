﻿using Amazon.S3.Model;
using BrandUp.FileStorage.Builder;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BrandUp.FileStorage.AwsS3
{
    public class MetadataSerializer<TMetadata> : IMetadataSerializer<TMetadata> where TMetadata : class, new()
    {
        const string metadataKey = "X-Amz-Meta";
        readonly PropertyInfo[] metadataProperties;

        static readonly Regex r = new("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z0-9])", RegexOptions.Compiled);

        public MetadataSerializer(IFileStorageBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));


            metadataProperties = builder.Properties[typeof(TMetadata).Name];
            //if (builder.Properties.TryGetValue(typeof(TMetadata).Name, out metadataProperties))
            //    throw new ArgumentException(typeof(TMetadata).Name);
        }

        public FileInfo<TMetadata> Deserialize(Guid fileId, GetObjectMetadataResponse response)
        {
            var fileMetadata = new TMetadata();

            foreach (var property in metadataProperties)
            {
                var converter = TypeDescriptor.GetConverter(property.PropertyType);

                if (property.PropertyType == typeof(string))
                    property.SetValue(fileMetadata, DecodeFileName(response.Metadata[metadataKey + "-" + ToKebabCase(property.Name)]));
                else
                    property.SetValue(fileMetadata, converter.ConvertFrom(response.Metadata[metadataKey + "-" + ToKebabCase(property.Name)]));
            }

            return new FileInfo<TMetadata> { Metadata = fileMetadata, Size = response.ContentLength, FileId = fileId };
        }

        public Dictionary<string, string> Serialize(TMetadata fileInfo)
        {
            var metadata = new Dictionary<string, string>();

            foreach (var property in metadataProperties)
            {
                if (property.PropertyType == typeof(string))
                    metadata.Add(metadataKey + "-" + ToKebabCase(property.Name), EncodeFileName(property.GetValue(fileInfo).ToString()));
                else
                    metadata.Add(metadataKey + "-" + ToKebabCase(property.Name), property.GetValue(fileInfo).ToString());
            }

            return metadata;
        }

        #region Helpers

        static string EncodeFileName(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            return Convert.ToHexString(System.Text.Encoding.UTF8.GetBytes(fileName));
        }
        static string DecodeFileName(string encodedValue)
        {
            if (encodedValue == null)
                throw new ArgumentNullException(nameof(encodedValue));

            return System.Text.Encoding.UTF8.GetString(Convert.FromHexString(encodedValue));
        }

        static string ToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return r.Replace(value, "-$1").Trim();
        }

        #endregion
    }

    public interface IMetadataSerializer<TMetadata> where TMetadata : class, new()
    {
        Dictionary<string, string> Serialize(TMetadata fileInfo);
        FileInfo<TMetadata> Deserialize(Guid fileId, GetObjectMetadataResponse response);
    }
}
