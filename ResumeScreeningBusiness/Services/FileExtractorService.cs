using Azure.AI.TextAnalytics;
using Azure;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using ResumeScreeningBusiness.Interfaces;
using ResumeScreeningBusiness.Models;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ResumeScreeningBusiness.Services
{
    public class FileExtractorService : IFileExtractorService
    {
        private readonly string _connectionString;
        public FileExtractorService(IConfiguration configuration)
        {
            //_connectionString = configuration.GetConnectionString("AzureBlobStorageConnectionString");
            _connectionString = "DefaultEndpointsProtocol=https;AccountName=myaistorageaccount125;AccountKey=DpUWPDpAilrNkGTux1cNJ7nu2XTKYueiPrA3g3SeYuypuA4YtYTfr/93m8QrMfH+55ibZj4QHqSm+AStaIVvkw==;EndpointSuffix=core.windows.net";
        }

        public async Task<FileUploadAndNLPResponse> ExtractTextAndGetResumeEntities(FileUploadModel model)
        {
            FileUploadAndNLPResponse fileUploadAndNLPResponse = new FileUploadAndNLPResponse();
            List<ResumeEntitiesResponse> listOfResumeEntitiesResponse = new List<ResumeEntitiesResponse>();
            try
            {
                if (model.File?.ContentType == "application/pdf")
                {
                    await ExtractTextFromPdf(model, listOfResumeEntitiesResponse);
                    if (listOfResumeEntitiesResponse != null && !string.IsNullOrEmpty(listOfResumeEntitiesResponse[0]?.CandidatePhoneNumber))
                    {
                        await UploadResume(model, fileUploadAndNLPResponse, listOfResumeEntitiesResponse, "pdf");
                    }
                }
                else if (model.File?.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
                {
                    string text = await ExtractTextFromDocx(model, listOfResumeEntitiesResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            
            return fileUploadAndNLPResponse;
        }

        private async Task UploadResume(FileUploadModel model, FileUploadAndNLPResponse fileUploadAndNLPResponse, List<ResumeEntitiesResponse> listOfResumeEntitiesResponse, string fileType)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("aiblob");
            string? fileName = !string.IsNullOrEmpty(listOfResumeEntitiesResponse[0]?.CandidateName) ? listOfResumeEntitiesResponse[0].CandidateName : "No Candidate Name";
            string filePath = listOfResumeEntitiesResponse[0].CandidatePhoneNumber + "/" + fileName + "." + fileType;
            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(filePath);

            // Upload file to blob storage
            using (MemoryStream memoryStream = new MemoryStream())
            {
                model.File.CopyTo(memoryStream);
                memoryStream.Position = 0;
                await blobClient.UploadAsync(memoryStream, true);
                fileUploadAndNLPResponse.filePath = filePath;
                fileUploadAndNLPResponse.listOfResumeEntitiesResponse = listOfResumeEntitiesResponse;
            }
        }

        private Task<string> ExtractTextFromDocx(FileUploadModel model, List<ResumeEntitiesResponse> listOfResumeEntitiesResponse)
        {
            throw new NotImplementedException();
        }

        private static async Task ExtractTextFromPdf(FileUploadModel model, List<ResumeEntitiesResponse> listOfResumeEntitiesResponse)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                model.File.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using (PdfReader reader = new PdfReader(memoryStream))
                {
                    using (PdfDocument pdfDoc = new PdfDocument(reader))
                    {
                        StringWriter textWriter = new StringWriter();

                        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                        {
                            ResumeEntitiesResponse resumeEntitiesResponse = await GetResumeEntities(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
                            listOfResumeEntitiesResponse.Add(resumeEntitiesResponse);
                        }
                    }
                }
            }
        }

        private static async Task<ResumeEntitiesResponse> GetResumeEntities(string document)
        {
            ResumeEntitiesResponse resumeEntitiesResponse = new ResumeEntitiesResponse();
            var endpoint = "https://myfirstlanguageservice7.cognitiveservices.azure.com/";
            var apiKey = "1ef4a248c2ea42df8be205294d84a80a";

            var client = new TextAnalyticsClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            string preprocessedText = document;
            try
            {
                Response<CategorizedEntityCollection> result = await client.RecognizeEntitiesAsync(preprocessedText, language: "en");

                // Post-process entities: Map back to original custom entities
                foreach (var entity in result.Value)
                {
                    string originalEntityText = entity.Text;

                    Console.WriteLine($"\t\tEntity: {originalEntityText}\tType: {entity.Category}\tSubcategory: {entity.SubCategory}");
                    Console.WriteLine($"\t\tScore: {entity.ConfidenceScore:F2}\tLength: {entity.Length}\tOffset: {entity.Offset}\n");

                    switch (entity.Category.ToString())
                    {
                        case "Person":
                            resumeEntitiesResponse.CandidateName = entity.Text;
                            break;
                        case "Email":
                            resumeEntitiesResponse.CandidateEmail = entity.Text;
                            break;
                        case "PhoneNumber":
                            resumeEntitiesResponse.CandidatePhoneNumber = entity.Text;
                            break;
                        case "Skill":
                        case "Product":
                            resumeEntitiesResponse.Skills.Add(entity.Text);
                            break;
                        case "DateTime":
                            if (entity.SubCategory == "Duration")
                                resumeEntitiesResponse.Experiences.Add(entity.Text);
                            else if(entity.SubCategory == "DateRange")
                                resumeEntitiesResponse.ExperienceRanges.Add(entity.Text);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return resumeEntitiesResponse;
        }

        public async Task<CloudBlockBlob> DownloadResume(string fileName)
        {
            try
            {
                CloudBlockBlob blockBlob;
                await using (MemoryStream memoryStream = new MemoryStream())
                {
                    CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_connectionString);
                    CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                    CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("aiblob");
                    blockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);
                    await blockBlob.DownloadToStreamAsync(memoryStream);
                }
                return blockBlob;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return null;
        }
    }
}
