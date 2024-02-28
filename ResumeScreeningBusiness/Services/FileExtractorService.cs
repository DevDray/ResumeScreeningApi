using Azure.AI.TextAnalytics;
using Azure;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;
using ResumeScreeningBusiness.Interfaces;
using ResumeScreeningBusiness.Models;

namespace ResumeScreeningBusiness.Services
{
    public class FileExtractorService : IFileExtractorService
    {
        public FileExtractorService()
        {

        }
        public async Task<string> ExtractText(FileUploadModel model)
        {

            string text = "";

            if (model.File.ContentType == "application/pdf")
            {
                text = ExtractTextFromPdf(model.File);
            }
            else if (model.File.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                text = await ExtractTextFromDocx(model.File);
            }

            return text;

        }

        private string ExtractTextFromPdf(IFormFile file)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                memoryStream.Position = 0;

                using (PdfReader reader = new PdfReader(memoryStream))
                {
                    using (PdfDocument pdfDoc = new PdfDocument(reader))
                    {
                        StringWriter textWriter = new StringWriter();

                        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                        {
                            textWriter.WriteLine(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
                        }

                        return textWriter.ToString();
                    }
                }
            }
        }

        private async Task<string> ExtractTextFromDocx(IFormFile file)
        {
            // Your implementation for extracting text from DOCX files using Open XML SDK
            return null;
        }

        public async Task<ResumeEntitiesResponse> ExtractTextAndGetResumeEntities(string document)
        {
            ResumeEntitiesResponse resumeEntitiesResponse = new ResumeEntitiesResponse();
            var endpoint = "https://myfirstlanguageservice5.cognitiveservices.azure.com/";
            var apiKey = "b2b75dcd292a4e6898b9c01815e78022";

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
                            resumeEntitiesResponse.DevelopmentSkills.Add(entity.Text);
                            break;
                        case "Product":
                            resumeEntitiesResponse.CloudSkills.Add(entity.Text);
                            break;
                        case "DateTime":
                            if (entity.SubCategory == "Duration")
                                resumeEntitiesResponse.Experiences.Add(entity.Text);
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
    }
}
