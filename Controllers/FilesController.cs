using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace awss3demo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;

        public FilesController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [HttpPost]
        public async Task<ActionResult> UploadFileAsync(IFormFile file, string bucketName, string? prefix)
        {
            var isBucketExisted = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!isBucketExisted)
            {
                return BadRequest($"Bucket {bucketName} does not exist.");
            }

            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                InputStream = file.OpenReadStream()
            };

            request.Metadata.Add("Content-Type", file.ContentType);
            await _s3Client.PutObjectAsync(request);

            return Ok($"File {prefix}/{file.FileName} uploaded to S3 successfully!");
        }

        [HttpGet]
        public async Task<ActionResult> GetAllFilesAsync(string bucketName, string? prefix)
        {
            var isBucketExisted = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!isBucketExisted)
            {
                return BadRequest($"Bucket {bucketName} does not exist.");
            }

            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var result = await _s3Client.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = s.Key,
                    Expires = DateTime.Now.AddMinutes(5)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key,
                    PresignedUrl = _s3Client.GetPreSignedURL(urlRequest)
                };
            });

            return Ok(s3Objects);
        }

        [HttpGet("{key}")]
        public async Task<ActionResult> GetFileByKeyAsync(string key, string bucketName)
        {
            var isBucketExisted = await _s3Client.DoesS3BucketExistAsync(bucketName);
            if (!isBucketExisted)
            {
                return BadRequest($"Bucket {bucketName} does not exist.");
            }

            var request = new GetObjectRequest()
            {
                BucketName = bucketName,
                Key = key
            };
            var result = await _s3Client.GetObjectAsync(request);
            var stream = result.ResponseStream;
            var fileName = result.Key;
            var contentType = result.Headers.ContentType;

            return File(stream, contentType, fileName);
        }
    }
}