using Azure.AI.TextAnalytics;
using Azure;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;
using ResumeScreeningBusiness.Interfaces;
using ResumeScreeningBusiness.Models;
using System.Collections.Generic;

namespace ResumeScreeningBusiness.Services
{
    public class FileExtractorService : IFileExtractorService
    {
        public FileExtractorService()
        {

        }

        public async Task<List<ResumeEntitiesResponse>> ExtractTextAndGetResumeEntities(FileUploadModel model)
        {
            string text = "";
            List<ResumeEntitiesResponse> listOfResumeEntitiesResponse = new List<ResumeEntitiesResponse>();

            if (model.File.ContentType == "application/pdf")
            {
                await ExtractTextFromPdf(model, listOfResumeEntitiesResponse);
            }
            else if (model.File.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document")
            {
                text = await ExtractTextFromDocx(model, listOfResumeEntitiesResponse);
            }

            
            return listOfResumeEntitiesResponse;
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
            var endpoint = "https://myfirstlanguageservice6.cognitiveservices.azure.com/";
            var apiKey = "ef13359bb5e54d8f958d18168d0bafe4";

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
