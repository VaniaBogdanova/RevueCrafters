using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using RevueCrafters1.Models;
using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace RevueCrafters1
{
    [TestFixture]
    public class RevueTests
    {

        private RestClient client;
        private const string baseUrl = "https://d2925tksfvgq8c.cloudfront.net";
        private static string lastCreatedRevueId;

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("vania456@gmail.com", "vania456");
            var options = new RestClientOptions(baseUrl)
            {

                Authenticator = new JwtAuthenticator(token)
            };
            client = new RestClient(options);
        }
        private string GetJwtToken(string email, string password)
        {
            var loginClient = new RestClient(baseUrl);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString() ?? string.Empty;
        }
        
        [Order(1)]
        [Test]

        public void CreateRevue_WithRequiredFields_ShouldReturnSuccess()
        {
            var revue = new
            {
                title = "New Revue",
                url = "",
                description = "Test revue description"
            };
            var request = new RestRequest("/api/Revue/Create", Method.Post);

            request.AddJsonBody(revue);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responsRevue = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(response.Content, Does.Contain("Successfully created!"));
        }

        [Order(2)]
        [Test]
        public void GetAllRevues_ShouldReturnListOfRevues()
        {
            var request = new RestRequest("/api/Revue/All", Method.Get);
            var response = client.Execute(request);
            var revues = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), response.Content);

            Assert.That(revues, Is.Not.Empty);
            Assert.That(revues.Count, Is.GreaterThan(0));
            lastCreatedRevueId = JsonSerializer.Deserialize<JsonElement>(response.Content)[0].GetProperty("id").GetString();
            Assert.That(lastCreatedRevueId, Is.Not.Null.Or.Empty, "Last created revue ID should not be null or empty.");


        }

        [Order(3)]
        [Test]
        public void EditExistingRevue_ShouldReturnSuccess()
        {
            var revueUpdate = new RevueDTO
            {
                Title = "Updated Revue Title",
                Url = "",
                Description = "Updated revue description"
            };

            var request = new RestRequest("/api/Revue/Edit", Method.Put);
            request.AddQueryParameter("revueId", lastCreatedRevueId);
            request.AddJsonBody(revueUpdate);

            var response = client.Execute(request);
            var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), response.Content);
            Assert.That(apiResponse.Msg, Is.EqualTo("Edited successfully"));
        }

        [Order(4)]
        [Test]
        public void DeleteRevue_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Revue/Delete", Method.Delete);
            request.AddQueryParameter("revueId", lastCreatedRevueId);
            var response = client.Execute(request);

            var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), response.Content);
            Assert.That(apiResponse.Msg, Is.EqualTo("The revue is deleted!"));

        }

        [Order(5)]
        [Test]

        public void CreateRevue_WithoutRequiredFields_ShouldReturnSuccessAgain()
        {
            var revue = new
            {
                title = "",
                url = "",
                description = ""
            };

            var request = new RestRequest("/api/Revue/Create", Method.Post);
            request.AddJsonBody(revue);

            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]

        public void EditNonExistingRevue_ShouldReturnNotFound()
        {
            string invalidRevueId = "invalid-id-123";
            var updatedRevue = new RevueDTO
            {
                Title = "Does Not Exist",
                Url = "",
                Description = "Fake revue"
            };

            var request = new RestRequest("/api/Revue/Edit", Method.Put);
            request.AddParameter("revueId", invalidRevueId, ParameterType.QueryString);
            request.AddJsonBody(updatedRevue);

            var response = client.Execute(request);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such revue!"));
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingIdea_ShouldReturnNotFound()
        {
            string invalidRevueId = "invalid-id-123"; 

            var request = new RestRequest("/api/Revue/Delete", Method.Delete);

            request.AddQueryParameter("revueId", invalidRevueId);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("There is no such revue!"));

        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
        
    }
}