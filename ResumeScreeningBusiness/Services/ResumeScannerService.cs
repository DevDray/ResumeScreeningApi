using Azure;
using Azure.AI.TextAnalytics;
using ResumeScreeningBusiness.Interfaces;
using ResumeScreeningBusiness.Models;

namespace ResumeScreeningBusiness.Services
{
    public class ResumeScannerService : IResumeScannerService
    {
        public async Task<ResumeEntitiesResponse> GetResumeEntities(string document)
        {
            ResumeEntitiesResponse resumeEntitiesResponse = new ResumeEntitiesResponse();
            var endpoint = "https://myfirstlanguageservice4.cognitiveservices.azure.com/";
            var apiKey = "b18870bf07ab4d62a8879afd2146d0bf";

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