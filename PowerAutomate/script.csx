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
    private const string API_ENDPOINT = "https://api.cradl.ai/v1";
    private const string AUTH_ENDPOINT = "https://auth.cradl.ai/oauth2/token";
    private const int MIN_RETRY_TIME_SECONDS = 20;
    private const int MAX_RETRY_TIME_SECONDS = 900;

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
              else if (path.StartsWith("/v1/metadata/")){
                  return await GetDocumentMetadata();
              }
              else if (path.StartsWith("/v1/actions/cradl:action:")){
                  return await TeardownTrigger();
              }
              else if (path.StartsWith("/v1/agents/cradl:agent:")){
                  return await PollAgentRun();
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
        bool waitForResult = true;

        if (request.Headers.TryGetValues("waitForResult", out var values) && bool.TryParse(values.First(), out var parsed)) {
            waitForResult = parsed;
        }

        string agentId = request.Headers.GetValues("AgentId").First();
        string variablesString = request.Headers.TryGetValues("variables", out var v) ? v.FirstOrDefault() : null;
        var fileContent = await this.Context.Request.Content.ReadAsByteArrayAsync();

        // Always add triggerSource to variables
        JObject variablesObj = new JObject();
        if (!string.IsNullOrEmpty(variablesString)) {
            try {
                variablesObj = JObject.Parse(variablesString);
            }
            catch (Exception ex) {
                return BadRequest($"Could not parse \"variables\" from headers as JSON: {ex.Message}");
            }
            request.Headers.Remove("variables");
        }
        // Add triggerSource regardless
        variablesObj["triggerSource"] = new JObject { ["value"] = "power-automate" };
        request.Content = CreateJsonContent(new JObject { ["variables"] = variablesObj }.ToString());

        // Redefine request and get response
        request.RequestUri = new Uri($"{Script.API_ENDPOINT}/agents/{agentId}/runs");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var response = await this.Context.SendAsync(request, this.CancellationToken);
        var content = await ToJson(response);
        string fullAgentRunId = (string) content["id"];

        // Create Document handle
        string fileName = request.Headers.TryGetValues("title", out var title) ? title.FirstOrDefault() : "Untitled";
        // Start CreateDocument task asynchronously
        var createDocumentTask = CreateDocument(fullAgentRunId, fileName, fileContent, accessToken);

        // Continue with the rest of the logic while document is uploading
        string actionId = null;
        bool isWaitForResultTrue = false;
        if (request.Headers.TryGetValues("ActionId", out var actionIdValues)) {
            actionId = actionIdValues.FirstOrDefault();
            if (!string.IsNullOrEmpty(actionId)) {
                var requestGetAction = CreateAuthorizedRequest(
                    method: HttpMethod.Get,
                    path: $"/actions/{actionId}",
                    accessToken: accessToken
                );
                var responseGetAction = await this.Context.SendAsync(requestGetAction, this.CancellationToken);
                var contentGetAction = await ToJson(responseGetAction);
                if (contentGetAction is JObject obj) {
                    var config = obj["config"] as JObject;
                    if (config != null && config["waitForResult"] != null) {
                        var waitForResultToken = config["waitForResult"];
                        if (waitForResultToken.Type == JTokenType.Boolean && waitForResultToken.Value<bool>() == true) {
                            isWaitForResultTrue = true;
                        }
                    }
                }
            }
        }

        if (isWaitForResultTrue && !string.IsNullOrEmpty(actionId))
        {
            string agentRunId = (string) content["runId"];
            string urlPrefix = request.Headers.GetValues("X-MS-APIM-Referrer-Prefix").First();
            int retryAfter = Script.MIN_RETRY_TIME_SECONDS;
            string actionIdQuery = System.Web.HttpUtility.UrlEncode(actionId);
            response.Headers.Add("Location", $"{urlPrefix}/agents/{agentId}/runs/{agentRunId}?actionId={actionIdQuery}");
            response.StatusCode = HttpStatusCode.Accepted;
            response.Headers.Add("Retry-After", retryAfter.ToString());
            response.Content = null;
        }

        // Await the document creation only when needed
        string documentId = await createDocumentTask;
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

        // Parse waitForResult and AgentId from query parameter
        bool? queryWaitForResult = null;
        string queryAgentId = null;
        var query = request.RequestUri.Query;
        if (!string.IsNullOrEmpty(query))
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(query);
            var waitForResultStr = queryParams.Get("waitForResult");
            var agentStr = queryParams.Get("AgentId");
            if (!string.IsNullOrEmpty(agentStr))
            {
                queryAgentId = agentStr;
            }
            if (!string.IsNullOrEmpty(waitForResultStr))
            {
                if (waitForResultStr == "true") {
                    queryWaitForResult = true;
                }
                else {
                    queryWaitForResult = false;
                }
            }
        }

        // Get Actions
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        var response = await this.Context.SendAsync(request, this.CancellationToken);
        var content = await ToJson(response);

        // Helper: get agent name if available
        JObject agents = null;
        if (string.IsNullOrEmpty(queryAgentId))
        {
            // Only fetch agents if not filtering by AgentId
            var requestGetAgents = CreateAuthorizedRequest(
                method: HttpMethod.Get,
                path: $"/agents",
                accessToken: accessToken
            );
            var responseGetAgents = await this.Context.SendAsync(requestGetAgents, this.CancellationToken);
            var contentGetAgents = await ToJson(responseGetAgents);
            agents = new JObject();
            foreach (var agent in contentGetAgents["agents"])
            {
                agents[agent["agentId"].ToString()] = agent["name"].ToString();
            }
        }

        // Common action filtering logic
        JArray exportActions = new JArray();
        var actions = content["actions"] as JArray;
        if (actions == null) {
            throw new Exception("No actions defined in your organizations");
        }

        foreach (var action in actions.OfType<JObject>())
        {
            var functionId = action["functionId"]?.ToString();
            var config = action["config"] as JObject;
            bool? actionWaitForResult = null;
            if (config != null && config["waitForResult"] != null) {
                if (bool.TryParse(config["waitForResult"].ToString(), out var parsedActionWait)) {
                    actionWaitForResult = parsedActionWait;
                }
            }
            bool match = true;
            if (queryWaitForResult.HasValue) {
                match = actionWaitForResult.HasValue && actionWaitForResult.Value == queryWaitForResult.Value;
            }
            var agentId = action["agentId"]?.ToString();
            if (!string.IsNullOrEmpty(queryAgentId))
            {
                // Only show actions from the specified agent
                if (functionId == "cradl:organization:cradl/cradl:function:export-to-power-automate" && match && agentId == queryAgentId)
                {
                    var actionName = action["name"]?.ToString() ?? "Unnamed action";
                    var actionId = action["actionId"]?.ToString();
                    var actionObj = new JObject {
                        ["actionId"] = actionId,
                        ["name"] = $"{actionName} from the selected agent"
                    };
                    exportActions.Add(actionObj);
                }
            }
            else
            {
                // Show actions from all agents, with agent name if available
                if (functionId == "cradl:organization:cradl/cradl:function:export-to-power-automate" && match)
                {
                    var actionName = action["name"]?.ToString() ?? "Unnamed action";
                    var actionId = action["actionId"]?.ToString();
                    if (!string.IsNullOrEmpty(agentId) &&
                        agents != null &&
                        agents.TryGetValue(agentId, out var agentValue) &&
                        agentValue != null)
                    {
                        var actionObj = new JObject {
                            ["actionId"] = actionId,
                            ["name"] = $"{actionName} from Agent \"{agentValue.ToString()}\""
                        };
                        exportActions.Add(actionObj);
                    }
                }
            }
        }
        content["actions"] = exportActions;

        if (exportActions.Count == 0)
        {
            // Compose a helpful message for the user
            string waitForResultMsg = "";
            if (queryWaitForResult.HasValue)
                waitForResultMsg = queryWaitForResult.Value.ToString().ToLower();
            else
                waitForResultMsg = "(not set)";

            content["message"] = $"No matching export actions found. Please go to the workflow setup of your agent and make sure you have a Power Automate export action with waitForResult={waitForResultMsg}.";
        }

        response.Content = CreateJsonContent(content.ToString());
        return response;
    }

    public static JObject FormatPredictions(JObject values, JObject fieldConfig) {
        var result = new JObject();

        // Non-prediction keys: keys in values not in fieldConfig
        foreach (var prop in values.Properties())
        {
            if (fieldConfig[prop.Name] == null)
            {
                result[prop.Name] = prop.Value;
            }
        }

        // Prediction keys: keys in both values and fieldConfig
        foreach (var prop in values.Properties())
        {
            if (fieldConfig[prop.Name] != null)
            {
                var fieldSpec = fieldConfig[prop.Name] as JObject;
                var type = fieldSpec["type"]?.ToString();
                if (type == "single-value")
                {
                    var valueObj = prop.Value as JObject;
                    if (valueObj != null)
                    {
                        valueObj["name"] = fieldSpec["name"] ?? prop.Name;
                        result[prop.Name] = valueObj;
                    }
                }
                else if (type == "table")
                {
                    var tableConfig = fieldSpec["fields"] as JObject;
                    var rowsArray = prop.Value as JArray;
                    if (tableConfig != null && rowsArray != null)
                    {
                        var formattedRows = new JArray();
                        foreach (var row in rowsArray)
                        {
                            if (row is JObject rowObj)
                            {
                                formattedRows.Add(FormatPredictions(rowObj, tableConfig));
                            }
                        }
                        result[prop.Name] = formattedRows;
                    }
                }
            }
        }

        return result;
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

    private static int CalculateRetryAfter(string updatedTime, HttpRequestMessage request) {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset eventTime;
        double secondsSinceEvent = 0;
        DateTimeOffset.TryParse(
            updatedTime,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal,
            out eventTime
        );
        secondsSinceEvent = (now - eventTime).TotalSeconds;

        // Get min/max wait time from
        int minRetry = Script.MIN_RETRY_TIME_SECONDS, maxRetry = Script.MAX_RETRY_TIME_SECONDS;
        if (request.Headers.TryGetValues("maxWaitInterval", out var maxVals))
            int.TryParse(maxVals.FirstOrDefault(), out maxRetry);

        // Do not allow users to configure max time to be less than the default minimum time
        maxRetry = Math.Max(Script.MIN_RETRY_TIME_SECONDS, maxRetry);

        // Calculate wait time: half the time since event, but clamp to [minRetry, maxRetry]
        int retryAfter = minRetry;
        if (secondsSinceEvent > 0)
        {
            retryAfter = (int)Math.Round(secondsSinceEvent / 4.0);
            retryAfter = Math.Max(minRetry, Math.Min(retryAfter, maxRetry));
        }
        return retryAfter;
    }

    private async Task<HttpResponseMessage> PollAgentRun()
    {
        // Get /agents/{agentId}/runs/{runId} to check if the run is completed
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.RequestUri = new Uri(Uri.UnescapeDataString($"{request.RequestUri}"));
        var response = await this.Context.SendAsync(request, this.CancellationToken);
        var responseJson = await ToJson(response);

        if (!response.IsSuccessStatusCode) {
            throw new Exception($"Could not poll agentRun: {request.RequestUri}");
        }

        // Get actionId from query parameter
        string actionId = null;
        var query = request.RequestUri.Query;
        if (!string.IsNullOrEmpty(query)) {
            var queryParams = System.Web.HttpUtility.ParseQueryString(query);
            actionId = queryParams.Get("actionId");
        }

        // Parse events to determine completion
        var events = responseJson["events"] as JArray;
        bool isCompleted = false;
        if (events != null && !string.IsNullOrEmpty(actionId)) {
            for (int i = events.Count - 1; i >= 0; i--) {
                var evt = events[i];
                var evtActionId = evt["actionId"]?.ToString();
                var resourceId = evt["resourceId"]?.ToString();
                var status = evt["status"]?.ToString();
                if (!string.IsNullOrEmpty(resourceId) &&
                    resourceId.StartsWith(actionId) &&
                    evtActionId == actionId &&
                    status == "running") {
                    isCompleted = true;
                    break;
                }
            }
        }

        if (!isCompleted) { // Keep polling
            string url = request.Headers.GetValues("X-MS-APIM-Referrer").First();
            // Calculate RetryAfter based on updatedTime or createdTime
            string updatedTimeStr = responseJson["updatedTime"]?.ToString();
            string createdTimeStr = responseJson["createdTime"]?.ToString();
            string timeStr = !string.IsNullOrEmpty(updatedTimeStr) ? updatedTimeStr : createdTimeStr;
            int retryAfter = CalculateRetryAfter(timeStr, request);

            var acceptedResponse = new HttpResponseMessage(HttpStatusCode.Accepted);
            acceptedResponse.Headers.Add("Location", $"{url}");
            acceptedResponse.Headers.Add("Retry-After", retryAfter.ToString());
            return acceptedResponse;
        }
        else {  // Return results from run
            // Get agent run variables
            JObject variables = null;
            var fileUrl = responseJson["variablesFileUrl"]?.ToString();
            var variablesRequest = CreateAuthorizedRequest(HttpMethod.Get, new Uri(fileUrl), accessToken);
            var variablesResponse = await this.Context.SendAsync(variablesRequest, this.CancellationToken);
            var variablesJson = await ToJson(variablesResponse);

            // Find documentId
            string documentId = null;
            var resources = responseJson["resourceIds"] as JArray;
            foreach (var resource in resources) {
                if (((string) resource).StartsWith("cradl:document")) {
                    documentId = resource.ToString();
                    break;
                }
            }

            // Find the modelId from events
            string modelId = null;
            if (events != null) {
                foreach (var evt in events) {
                    if (evt["modelId"] != null) {
                        modelId = evt["modelId"].ToString();
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(modelId)) {
                throw new Exception("No event with modelId found in events. Please contact support@cradl.ai");
            }

            // Get fieldConfig for Model
            var requestGetModel = CreateAuthorizedRequest(
                method: HttpMethod.Get,
                path: $"/models/{modelId}",
                accessToken: accessToken
            );
            var responseGetModel = await this.Context.SendAsync(requestGetModel, this.CancellationToken);
            JObject contentGetModel = await ToJson(responseGetModel);

            // Format and return variables according to the fieldConfig
            variables = FormatPredictions(variablesJson, (JObject) contentGetModel["fieldConfig"]);

            if (variables == null) {
                throw new Exception($"Could not find variables. please contact support@cradl.ai");
            }

            return new HttpResponseMessage(HttpStatusCode.OK) {
                Content = CreateJsonContent(new JObject {
                    ["output"] = variables,
                    ["context"] = new JObject {
                        ["runId"] = responseJson["runId"],
                        ["documentId"] = documentId
                    }
                }.ToString())
            };
        }
    }

    private async Task<HttpResponseMessage> GetSchema()
    {
        var request = this.Context.Request;
        string accessToken = await GetAccessToken();
        string agentId = request.Headers.TryGetValues("AgentId", out var v) ? v.FirstOrDefault() : null;

        if (string.IsNullOrWhiteSpace(agentId)) { // Find agentId from actionId
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            string actionId = request.Headers.TryGetValues("ActionId", out var va) ? va.FirstOrDefault() : null;

            if (string.IsNullOrWhiteSpace(actionId)) {
                throw new Exception("ActionId header is empty. Please contact support@cradl.ai");
            }

            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri($"{Script.API_ENDPOINT}/actions/{actionId}");
            var responseGetAction = await this.Context.SendAsync(request, this.CancellationToken);
            var contentGetAction = await ToJson(responseGetAction);
            agentId = contentGetAction["agentId"]?.ToString();

            if (!responseGetAction.IsSuccessStatusCode) {
                throw new Exception($"{actionId} is not valid, try refreshing or contact support@cradl.ai");
            }
            else if (string.IsNullOrWhiteSpace(agentId)) {
                throw new Exception($"agentId is missing in action {actionId}. Create a new trigger/export in Cradl or contact support@cradl.ai");
            }
        }

        // Get agent to find modelId
        var requestGetAgents = CreateAuthorizedRequest(
            method: HttpMethod.Get,
            path: $"/agents/{agentId}",
            accessToken: accessToken
        );
        var responseGetAgents = await this.Context.SendAsync(requestGetAgents, this.CancellationToken);
        var content = await ToJson(responseGetAgents);
        var resources = content["resourceIds"] as JArray;

        if (resources == null || resources.Count == 0) {
            throw new Exception($"resourceIds not found in /agents/{agentId} response. Create a new agent or contact support@cradl.ai");
        }

        // Find the model among the resourceIds (it should start with "cradl:model")
        string modelId = null;
        foreach (var resource in resources) {
            if (((string) resource).StartsWith("cradl:model")) {
                modelId = resource.ToString();
                break;
            }
        }

        if (string.IsNullOrEmpty(modelId)) {
            throw new Exception($"Could not find a model in ${agentId}. Create a new agent or contact support@cradl.ai");
        }

        // Get JSON schema for Power Automate from the field config of the model
        var requestGetModel = CreateAuthorizedRequest(
            method: HttpMethod.Get,
            path: $"/models/{modelId}",
            accessToken: accessToken
        );
        HttpResponseMessage responseGetModel = await this.Context.SendAsync(requestGetModel, this.CancellationToken);
        JObject contentGetModel = await ToJson(responseGetModel);
        var schema = CreateJsonSchema((JObject) contentGetModel["fieldConfig"]);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(schema.ToString(), Encoding.UTF8, "application/json")
        };
        response.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MaxAge = TimeSpan.Zero,
            MustRevalidate = true
        };
        response.Headers.Pragma.ParseAdd("no-cache");
        response.Content.Headers.Expires = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        return response;
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

        // Set Location in header to allow teardown of the trigger
        response.Headers.Add("location", $"{Script.API_ENDPOINT}/actions/{actionId}");
        response.Headers.CacheControl = new CacheControlHeaderValue
        {
            NoCache = true,
            NoStore = true,
            MaxAge = TimeSpan.Zero,
            MustRevalidate = true
        };
        response.Headers.Pragma.ParseAdd("no-cache");
        response.Content.Headers.Expires = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

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
            // Get the hmacSecret
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

            // Get signature, URL, headers, and body from the incoming request
            string receivedSharedSecret = this.Context.Request.Headers.TryGetValues("Cradl-Shared-Secret", out var v) ? v.FirstOrDefault() : null;
            if (string.IsNullOrEmpty(receivedSharedSecret)) {
                return BadRequest("Missing Cradl-Shared-Secret in header.");
            }

            // Compare to signature
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

