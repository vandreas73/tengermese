using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Seo;
using SkiaSharp;
using static System.Reflection.Metadata.BlobBuilder;

namespace Nop.Services.Media.RoxyFileman
{
    public class RoxyAzurePictureService
    {

        #region Fields

        private static BlobContainerClient _blobContainerClient;
        private static BlobServiceClient _blobServiceClient;
        private static bool _azureBlobStorageAppendContainerName;
        private static bool _isInitialized;
        private static string _azureBlobStorageConnectionString;
        private static string _azureBlobStorageContainerName;
        private static string _azureBlobStorageEndPoint;

        private readonly IStaticCacheManager _staticCacheManager;
        private readonly MediaSettings _mediaSettings;

        private readonly object _locker = new();

        #endregion

        #region Ctor

        public RoxyAzurePictureService(AppSettings appSettings,
            IStaticCacheManager staticCacheManager,
            MediaSettings mediaSettings)
        {
            _staticCacheManager = staticCacheManager;
            _mediaSettings = mediaSettings;

            OneTimeInit(appSettings);
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Initialize cloud container
        /// </summary>
        /// <param name="appSettings">App settings</param>
        protected void OneTimeInit(AppSettings appSettings)
        {
            if (_isInitialized)
                return;

            if (string.IsNullOrEmpty(appSettings.Get<AzureBlobConfig>().ConnectionString))
                throw new Exception("Azure connection string for Blob is not specified");

            if (string.IsNullOrEmpty(appSettings.Get<AzureBlobConfig>().ContainerName))
                throw new Exception("Azure container name for Blob is not specified");

            if (string.IsNullOrEmpty(appSettings.Get<AzureBlobConfig>().EndPoint))
                throw new Exception("Azure end point for Blob is not specified");

            lock (_locker)
            {
                if (_isInitialized)
                    return;

                _azureBlobStorageAppendContainerName = appSettings.Get<AzureBlobConfig>().AppendContainerName;
                _azureBlobStorageConnectionString = appSettings.Get<AzureBlobConfig>().ConnectionString;
                _azureBlobStorageContainerName = appSettings.Get<AzureBlobConfig>().ContainerName.Trim().ToLowerInvariant();
                _azureBlobStorageEndPoint = appSettings.Get<AzureBlobConfig>().EndPoint.Trim().ToLowerInvariant().TrimEnd('/');

                _blobServiceClient = new BlobServiceClient(_azureBlobStorageConnectionString);
                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(_azureBlobStorageContainerName);

                CreateCloudBlobContainer().GetAwaiter().GetResult();

                _isInitialized = true;
            }
        }

        /// <summary>
        /// Create cloud Blob container
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CreateCloudBlobContainer()
        {
            await _blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }

        /// <summary>
        /// Get picture (thumb) local path
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the local picture thumb path
        /// </returns>
        protected Task<string> GetThumbLocalPathAsync(string thumbFileName)
        {
            var path = _azureBlobStorageAppendContainerName ? $"{_azureBlobStorageContainerName}/" : string.Empty;

            return Task.FromResult($"{_azureBlobStorageEndPoint}/{path}{thumbFileName}");
        }

        /// <summary>
        /// Get picture (thumb) URL 
        /// </summary>
        /// <param name="thumbFileName">Filename</param>
        /// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the local picture thumb path
        /// </returns>
        public async Task<string> GetImagesUrlAsync(string thumbFileName, string storeLocation = null)
        {
            return await GetThumbLocalPathAsync(thumbFileName);
        }

        public async Task SaveImageAsync(string imageFilePath, string imageFileName, string mimeType, byte[] binary)
        {
            var blobClient = _blobContainerClient.GetBlobClient(imageFileName);
            await using var ms = new MemoryStream(binary);

            //set mime type
            BlobHttpHeaders headers = null;
            if (!string.IsNullOrWhiteSpace(mimeType))
            {
                headers = new BlobHttpHeaders
                {
                    ContentType = mimeType
                };
            }

            //set cache control
            if (!string.IsNullOrWhiteSpace(_mediaSettings.AzureCacheControlHeader))
            {
                headers ??= new BlobHttpHeaders();
                headers.CacheControl = _mediaSettings.AzureCacheControlHeader;
            }

            if (headers is null)
                //We must explicitly indicate through the parameter that the object needs to be overwritten if it already exists
                //See more: https://github.com/Azure/azure-sdk-for-net/issues/9470
                await blobClient.UploadAsync(ms, overwrite: true);
            else
                await blobClient.UploadAsync(ms, new BlobUploadOptions { HttpHeaders = headers });

            await _staticCacheManager.RemoveByPrefixAsync(NopMediaDefaults.ThumbsExistsPrefix);
        }

        public List<RoxyImageInfo> ListPicturesAsync()
        {
            List<RoxyImageInfo> imageInfos = new();
            foreach (BlobItem blob in _blobContainerClient.GetBlobs(BlobTraits.All, BlobStates.All))
            {
                imageInfos.Add(new RoxyImageInfo(blob.Name, blob.Properties.LastModified ?? DateTime.Now, blob.Properties.ContentLength ?? 0, 500, 500));
            }
            return imageInfos;
        }

        public void DeletePictureAsync(string path)
        {

            foreach (var blob in _blobContainerClient.GetBlobs(BlobTraits.All, BlobStates.All, path))
            {
                _blobContainerClient.DeleteBlobIfExistsAsync(blob.Name, DeleteSnapshotsOption.IncludeSnapshots);
            }
        }

        #endregion
    }
}
