using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;


public class Script : ScriptBase
{
    private const string API_ENDPOINT = "https://api.lucidtech.ai/v1";
    private const string AUTH_ENDPOINT = "https://auth.cradl.ai/oauth2/token";

    public override async Task<HttpResponseMessage> ExecuteAsync()
    {
        var path = Uri.UnescapeDataString(this.Context.Request.RequestUri.AbsolutePath.ToString());
        switch (path) {
            case "/v1/validate":
                return await Validate();
                break;
            case "/v1/agents":
                if (this.Context.Request.Method == HttpMethod.Post) {
                    return await CreateRun();
                }
                else {
                    return await GetAgents();
                };
                break;
            case "/v1/schema":
                return await GetSchema();
                break;
            case "/v1/actions":
                if (this.Context.Request.Method == HttpMethod.Get) {
                    return await GetActions();
                }
                else if (this.Context.Request.Method == HttpMethod.Post) {
                    return await SetupTrigger();
                }
                break;
            case "/v1/models":
                return await GetModelsDeprecated();
                break;
            case "/v1/workflows":
                return await CreateExecutionDeprecated();
                break;
            case "/v1/documents": 
                return await CreateDocumentDeprecated();
                break;
            default: 
              if (path.StartsWith("/v1/documents/")){
                  return await GetDocument();
              }
              if (path.StartsWith("/v1/metadata/")){
                  return await GetDocumentMetadata();
              }
              else if (path.StartsWith("/v1/actions/cradl:action:")){
                  return await TeardownTrigger();
              }
              else {
                throw new ArgumentException($"{path} is not assigned to any method");
              }
              break;

        }
        return null;
    }

    private async Task<HttpResponseMessage> GetDocument()
    {
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.RequestUri = new Uri(Uri.UnescapeDataString($"{request.RequestUri}"));
        var getDocumentResponse = await this.Context.SendAsync(request, this.CancellationToken);

        // Get document metadata
        var metadata = await ToJson(getDocumentResponse);
        var fileUrl = (string)metadata["fileUrl"];

        // Download file content
        var getRequest = CreateAuthorizedRequest(HttpMethod.Get, new Uri(fileUrl), accessToken);
        var getResponse = await this.Context.SendAsync(getRequest, this.CancellationToken);
        var fileBytes = await getResponse.Content.ReadAsByteArrayAsync();

        // Return raw file bytes
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new ByteArrayContent(fileBytes)
        };

        // Copy content type from the file server response (or force application/pdf, etc.)
        var contentType = (string)metadata["contentType"] ?? "application/octet-stream";
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        // If you still want metadata, add it as headers
        response.Headers.Add("Cradl-Document-Id", (string)metadata["DocumentId"]);
        response.Headers.Add("Cradl-Name", (string)metadata["name"]);

        return response;
    }
    
    private async Task<HttpResponseMessage> GetDocumentMetadata()
    {
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.RequestUri = new Uri(Uri.UnescapeDataString($"{request.RequestUri}".Replace("metadata", "documents")));
        var response = await this.Context.SendAsync(request, this.CancellationToken);
        return response;
    }
    
    private async Task<string> CreateDocument(string agentRunId, string fileName, byte[] fileContent, string accessToken)
    {
        // Create document handle
        var requestPostDocuments = CreateAuthorizedRequest(
            method: HttpMethod.Post,
            path: "/documents",
            accessToken: accessToken
        );
        var contentRequest = new JObject { ["agentRunId"] = agentRunId };

        if (!string.IsNullOrEmpty(fileName)) {
            contentRequest["name"] = fileName;
        }

        requestPostDocuments.Content = CreateJsonContent(contentRequest.ToString());
        var createDocumentResponse = await this.Context.SendAsync(requestPostDocuments, this.CancellationToken);
        var content = await ToJson(createDocumentResponse);
        
        // Upload document to fileserver
        var fileUrl = (string) content["fileUrl"];
        var putRequest = CreateAuthorizedRequest(HttpMethod.Put, new Uri(fileUrl), accessToken);
        putRequest.Content = new ByteArrayContent(fileContent);
        var putResponse = await this.Context.SendAsync(putRequest, this.CancellationToken);
        
        if (putResponse.IsSuccessStatusCode) {
            return (string) content["documentId"];
        }
        else {
            throw new Exception($"Could not create document with content: {content}");
        }
        
    }

    private async Task<HttpResponseMessage> CreateRun()
    {
        // Create Agent Run
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();

        // Get information from content and headers
        string agentId = request.Headers.GetValues("AgentId").First();
        string variablesString = request.Headers.TryGetValues("variables", out var v) ? v.FirstOrDefault() : null;
        var fileContent = await this.Context.Request.Content.ReadAsByteArrayAsync();

        // Redefine request and get response
        if (!string.IsNullOrEmpty(variablesString)) {
            try {
                var variables = JObject.Parse(variablesString);
                request.Content = CreateJsonContent(new JObject { ["variables"] = variables }.ToString());
            }
            catch (Exception ex) {
                return BadRequest($"Could not parse \"variables\" from headers as JSON: {ex.Message}");
            }
            request.Headers.Remove("variables");
        }
        else {
            request.Content = CreateJsonContent(new JObject {}.ToString());
        }
        request.RequestUri = new Uri($"{Script.API_ENDPOINT}/agents/{agentId}/runs");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var response = await this.Context.SendAsync(request, this.CancellationToken);       
        var content = await ToJson(response);
        string agentRunId = (string) content["id"];

        // Create Document handle
        string fileName = request.Headers.TryGetValues("title", out var title) ? title.FirstOrDefault() : "Untitled";
        string documentId = await CreateDocument(agentRunId, fileName, fileContent, accessToken);
        response.Headers.Add("documentId", documentId);
        return response;
    }
    
    private async Task<HttpResponseMessage> GetAgents()
    {
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        return await this.Context.SendAsync(request, this.CancellationToken);
    }

    private async Task<HttpResponseMessage> GetActions()
    {
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();

        // Get agents to separate the different actions from one another
        var requestGetAgents = CreateAuthorizedRequest(
            method: HttpMethod.Get,
            path: $"/agents",
            accessToken: accessToken
        );
        var responseGetAgents = await this.Context.SendAsync(requestGetAgents, this.CancellationToken);
        var contentGetAgents = await ToJson(responseGetAgents);
        JObject agents = new JObject();
        foreach (var agent in contentGetAgents["agents"]) {
            agents[agent["agentId"].ToString()] = agent["name"].ToString();
        }

        // Get Actions
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var response = await this.Context.SendAsync(request, this.CancellationToken);
        var content = await ToJson(response);
        
        // Filter actions and add agent name as additional info
        JArray exportActions = new JArray();
        foreach (var action in content["actions"]) {
            if ((string) action["functionId"] == "cradl:organization:cradl/cradl:function:export-to-power-automate"){
              string action_name = action["name"]?.ToString();
              string agent_name = agents[action["agentId"].ToString()].ToString();
              action["name"] = $"{action_name} from Agent \"{agent_name}\"";
              exportActions.Add(action);
            }
        }
        content["actions"] = exportActions;
        response.Content = CreateJsonContent(content.ToString());
        return response;
    }

    public static JObject CreateJsonSchema(JObject fieldConfig)
    {
        var root = new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject()
        };

        var rootProperties = (JObject)root["properties"];
        rootProperties["context"] = new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject {
               ["documentId"] = new JObject { ["type"] = "string" },
               ["runId"] = new JObject { ["type"] = "string" }
            }
        };
        var output = new JObject
        {
            ["type"] = "object",
            ["properties"] = new JObject()
        };
        rootProperties["output"] = output;

        var outputProperties = (JObject)output["properties"];

        foreach (var field in fieldConfig)
        {
            var fieldKey = field.Key;
            var fieldValue = (JObject)field.Value;

            if (fieldValue["type"]?.ToString() == "table")
            {
                // Handle table (array of objects)
                var tableSchema = new JObject
                {
                    ["type"] = "array",
                    ["title"] = fieldValue["name"]?.ToString() + " array",
                    ["description"] = "Array (Each element represents a row with predictions)",
                    ["items"] = new JObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JObject(),
                        ["description"] = "Items for each line"
                    }
                };

                var tableFields = (JObject)fieldValue["fields"];
                var tableProperties = (JObject)tableSchema["items"]["properties"];

                foreach (var subField in tableFields)
                {
                    var subKey = subField.Key;
                    var subValue = (JObject)subField.Value;

                    var description = subValue["promptHint"]?.ToString() ?? subValue["description"]?.ToString();
                    tableProperties[subKey] = CreateSingleValueSchema(subValue["name"]?.ToString(), description);
                }

                outputProperties[fieldKey] = tableSchema;
            }
            else
            {
                // Handle single-value fields
                var description = fieldValue["promptHint"]?.ToString() ?? fieldValue["description"]?.ToString();
                outputProperties[fieldKey] = CreateSingleValueSchema(fieldValue["name"]?.ToString(), description);
            }
        }

        return root;
    }

    private static JObject CreateSingleValueSchema(string fieldName, string description)
    {
        return new JObject
        {
            ["type"] = "object",
            ["title"] = fieldName,
            ["description"] = $"Properties for {fieldName}",
            ["properties"] = new JObject
            {
                ["value"] = new JObject
                {
                    ["title"] = "Value",
                    ["type"] = "string",
                    ["description"] = description
                }
            }
        };
    }

    private async Task<HttpResponseMessage> GetSchema()
    {
        // Find agentId
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        string actionId = request.Headers.GetValues("ActionId").First();
        request.Method = HttpMethod.Get;
        request.RequestUri = new Uri($"{Script.API_ENDPOINT}/actions/{actionId}");
        var responseGetAction = await this.Context.SendAsync(request, this.CancellationToken);
        var contentGetAction = await ToJson(responseGetAction);
        string agentId = (string) contentGetAction["agentId"];

        var requestGetAgents = CreateAuthorizedRequest(
            method: HttpMethod.Get,
            path: $"/agents/{agentId}",
            accessToken: accessToken
        );
        var responseGetAgents = await this.Context.SendAsync(requestGetAgents, this.CancellationToken);
        var content = await ToJson(responseGetAgents);

        foreach (var resource in content["resourceIds"]) {
            if (((string) resource).StartsWith("cradl:model")) {
              var requestGetModel = CreateAuthorizedRequest(
                  method: HttpMethod.Get,
                  path: $"/models/{resource}",
                  accessToken: accessToken
              );
              HttpResponseMessage responseGetModel = await this.Context.SendAsync(requestGetModel, this.CancellationToken);
              JObject contentGetModel = await ToJson(responseGetModel);
              var schema = CreateJsonSchema((JObject) contentGetModel["fieldConfig"]);
              var response = new HttpResponseMessage(HttpStatusCode.OK)
              {
                  Content = new StringContent(schema.ToString(), Encoding.UTF8, "application/json")
              };
              return response;
            }
        }
        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
    }
    
    private async Task<HttpResponseMessage> SetupTrigger()
    {
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();
        string actionId = request.Headers.GetValues("ActionId").First();

        // Parse existing content
        var content = JObject.Parse(await request.Content.ReadAsStringAsync());

        // Modify headers
        var headers = (JArray)content["config"]?["headers"] ?? new JArray();
        headers.Add(new JObject
        {
            ["key"] = "Cradl-Shared-Secret",
            ["value"] = Guid.NewGuid().ToString()
        });

        // Reassign headers back to config
        content["config"]["headers"] = headers;

        // Build PATCH request
        request.RequestUri = new Uri($"{Script.API_ENDPOINT}/actions/{actionId}");
        request.Method = new HttpMethod("PATCH");
        request.Headers.Remove("Authorization"); // in case it's already present
        request.Headers.Add("Authorization", $"Bearer {accessToken}");

        // Set updated JSON content
        request.Content = new StringContent(content.ToString(), Encoding.UTF8, "application/json");

        // Send the request
        var response = await this.Context.SendAsync(request, this.CancellationToken);

        // Optionally set a response header to allow teardown of the trigger 
        response.Headers.Add("Location", $"{Script.API_ENDPOINT}/actions/{actionId}");

        return response;
    }

    private async Task<HttpResponseMessage> TeardownTrigger()
    {
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();
        request.Method = new HttpMethod("PATCH");
        request.RequestUri = new Uri(Uri.UnescapeDataString($"{request.RequestUri}"));
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Content = CreateJsonContent(new JObject { 
            ["enabled"] = false,
            ["config"] = new JObject {
                ["url"] = null,
                ["headers"] = null,
                ["params"] = null,
                ["httpMethod"] = null
            }
        }.ToString());
        return await this.Context.SendAsync(request, this.CancellationToken);
    }

    private async Task<HttpResponseMessage> Validate()
    {
        try {
            // 1. Get the hmacSecret
            string actionId = this.Context.Request.Headers.GetValues("ActionId").First();
            if (string.IsNullOrEmpty(actionId)) {
                return BadRequest("Missing ActionId header.");
            }

            var request = CreateAuthorizedRequest(
                method: HttpMethod.Get,
                path: $"/actions/{actionId}",
                accessToken: await GetAccessToken()
            );

            var getActionResponse = await this.Context.SendAsync(request, this.CancellationToken);
            var contentGetAction = await ToJson(getActionResponse);


            var headers = (JArray) contentGetAction?["config"]?["headers"];
            string sharedSecret = ""; 
            foreach (var header in headers) {
                if (header["key"].ToString() == "Cradl-Shared-Secret") {
                    sharedSecret = header["value"].ToString();
                    break;
                }
            }

            if (string.IsNullOrEmpty(sharedSecret)) {
                return BadRequest("The secret has not been defined during setup.");
            }

            // 2. Get signature, URL, headers, and body from the incoming request
            string receivedSharedSecret = this.Context.Request.Headers.TryGetValues("Cradl-Shared-Secret", out var v) ? v.FirstOrDefault() : null;
            if (string.IsNullOrEmpty(receivedSharedSecret)) {
                return BadRequest("Missing Cradl-Shared-Secret in header.");
            }

            // 4. Compare to signature
            if (!string.Equals(sharedSecret, receivedSharedSecret, StringComparison.OrdinalIgnoreCase)) {
                return BadRequest($"Invalid secret: {receivedSharedSecret}.");
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
        catch (Exception ex) {
            return BadRequest($"Unexpected error: {ex.Message}");
        }
    }

    private HttpResponseMessage BadRequest(string message) {
        return new HttpResponseMessage(HttpStatusCode.BadRequest) {
            Content = CreateJsonContent(new JObject { ["Error"] = message }.ToString())
        };
    }

    private async Task<HttpResponseMessage> CreateExecutionDeprecated()
    {
        var request = this.Context.Request;
        string workflowId = request.Headers.GetValues("WorkflowId").First();
        request.RequestUri = new Uri($"{Script.API_ENDPOINT}/workflows/{workflowId}/executions");
        return await this.Context.SendAsync(request, this.CancellationToken);       
    }
    
    private async Task<HttpResponseMessage> GetModelsDeprecated()
    {
        var myModelsRes = this.Context.SendAsync(
            CreateAuthorizedRequestDeprecated(
                method: HttpMethod.Get,
                path: "/models"
            ),
            this.CancellationToken
        );

        var publicModelsRes = this.Context.SendAsync(
            CreateAuthorizedRequestDeprecated(
                method: HttpMethod.Get,
                path: "/models?owner=las:organization:cradl"
            ),
            this.CancellationToken
        );

        var response = await myModelsRes;
        var content = await ToJson(response);
        
        var myModels = (JArray) content["models"];
        foreach (var pretrainedModel in (await ToJson(await publicModelsRes))["models"]) {
            pretrainedModel["modelId"] = "las:organization:cradl/" + pretrainedModel["modelId"];
            myModels.Add(pretrainedModel);
        }

        response.Content = CreateJsonContent(content.ToString());
        return response;
    }

    private async Task<HttpResponseMessage> CreateDocumentDeprecated()
    {
        string fileName = this.Context.Request.Headers.GetValues("Name").First();
        var fileContent = await this.Context.Request.Content.ReadAsByteArrayAsync();

        var request = CreateAuthorizedRequestDeprecated(
            method: HttpMethod.Post,
            path: "/documents"
        );

        request.Content = CreateJsonContent(new JObject { ["name"] = fileName }.ToString());
        var response = await this.Context.SendAsync(request, this.CancellationToken);
        
        var fileUrl = (string) (await ToJson(response))["fileUrl"];
        
        var putRequest = new HttpRequestMessage(HttpMethod.Put, new Uri(fileUrl));
        putRequest.Headers.Add("Authorization", this.Context.Request.Headers.GetValues("Authorization").First());
        putRequest.Content = new ByteArrayContent(fileContent);
        var putResponse = await this.Context.SendAsync(putRequest, this.CancellationToken);
        
        if (!putResponse.IsSuccessStatusCode) {
            return putResponse;
        }
        
        return response;
    }
    
    private HttpRequestMessage CreateAuthorizedRequestDeprecated(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, new Uri($"{Script.API_ENDPOINT}{path}"));
        request.Headers.Add("Authorization", this.Context.Request.Headers.GetValues("Authorization").First());
        return request;
    }

    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, Uri uri, string accessToken)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        return request;
    }
    
    private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string path, string accessToken)
    {
        return CreateAuthorizedRequest(method, new Uri($"{Script.API_ENDPOINT}{path}"), accessToken);
    }
    
    
    private static async Task<JObject> ToJson(HttpResponseMessage response)
    {
        return JObject.Parse(await response.Content.ReadAsStringAsync());
    }

    private async Task<string> GetAccessToken()
    {
        // Decode apiKey (base64 encoded string "<clientId>:<clientSecret>")
        var apiKey = this.Context.Request.Headers.GetValues("apiKey").First();
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(apiKey));
        var parts = decoded.Split(':');
        if (parts.Length != 2) {
            throw new ArgumentException("Invalid API key format. Expected base64 encoded '<clientId>:<clientSecret>'");
        }

        var clientId = parts[0];
        var clientSecret = parts[1];

        // Prepare form-urlencoded content
        var formData = new[] {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("audience", "https://api.cradl.ai/v1")
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(Script.AUTH_ENDPOINT)) {
            Content = new FormUrlEncodedContent(formData)
        };

        using var response = await this.Context.SendAsync(request, this.CancellationToken);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JObject.Parse(content);
        var token = jsonResponse["access_token"]?.ToString();

        if (string.IsNullOrEmpty(token)) {
            throw new Exception("Access token was not found in the response.");
        }

        return token;
    }
}

