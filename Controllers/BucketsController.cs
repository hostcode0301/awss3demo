using Amazon.S3;
using Microsoft.AspNetCore.Mvc;

namespace awss3demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BucketsController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;

        public BucketsController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [HttpPost]
        public async Task<ActionResult> CreateBucketAsync(string bucketName)
        {
            var isExisted = await _s3Client.DoesS3BucketExistAsync(bucketName);

            if (!isExisted)
            {
                return BadRequest($"Bucket {bucketName} already exists.");
            }

            await _s3Client.PutBucketAsync(bucketName);

            return Ok($"Bucket {bucketName} created.");
        }

        [HttpGet]
        public async Task<ActionResult> GetAllBucketsAsync()
        {
            var buckets = await _s3Client.ListBucketsAsync();

            return Ok(buckets);
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteBucketAsync(string bucketName)
        {
            var isExisted = await _s3Client.DoesS3BucketExistAsync(bucketName);

            if (!isExisted)
            {
                return BadRequest($"Bucket {bucketName} does not exist.");
            }

            await _s3Client.DeleteBucketAsync(bucketName);

            return Ok($"Bucket {bucketName} deleted.");
        }
    }
}