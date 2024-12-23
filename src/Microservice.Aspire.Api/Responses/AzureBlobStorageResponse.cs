namespace Microservice.Aspire.Api.Responses
{
    public class AzureBlobStorageResponse : ResponseBase
    {
        public string? BlobUri { get; private set; }

        public byte[]? FileData { get; set; }

        public static AzureBlobStorageResponse Success(string message, string blobUri)
        {
            return new AzureBlobStorageResponse
            {
                IsValid = true,
                Message = message,
                BlobUri = blobUri
            };
        }

        public static AzureBlobStorageResponse Success(string message, byte[] fileData)
        {
            return new AzureBlobStorageResponse
            {
                IsValid = true,
                Message = message,
                FileData = fileData
            };
        }

        public static AzureBlobStorageResponse Failure(string message)
        {
            return new AzureBlobStorageResponse
            {
                IsValid = false,
                Message = message
            };
        }

        public static AzureBlobStorageResponse Failure(Exception ex)
        {
            return new AzureBlobStorageResponse
            {
                IsValid = false,
                Message = ex.Message,
                Exception = ex
            };
        }
    }
}
