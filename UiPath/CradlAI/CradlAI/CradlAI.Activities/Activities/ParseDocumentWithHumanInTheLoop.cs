using System;
using System.Activities;
using System.Threading;
using System.Threading.Tasks;
using CradlAI.Activities.Properties;
using UiPath.Shared.Activities;
using UiPath.Shared.Activities.Localization;
using Lucidtech.Las;
using Lucidtech.Las.Core;
using Lucidtech.Las.Utils;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CradlAI.Activities
{
    [LocalizedDisplayName(nameof(Resources.ParseDocumentWithHumanInTheLoop_DisplayName))]
    [LocalizedDescription(nameof(Resources.ParseDocumentWithHumanInTheLoop_Description))]
    public class ParseDocumentWithHumanInTheLoop : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.ParseDocumentWithHumanInTheLoop_ClientId_DisplayName))]
        [LocalizedDescription(nameof(Resources.ParseDocumentWithHumanInTheLoop_ClientId_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ClientId { get; set; }

        [LocalizedDisplayName(nameof(Resources.ParseDocumentWithHumanInTheLoop_ClientSecret_DisplayName))]
        [LocalizedDescription(nameof(Resources.ParseDocumentWithHumanInTheLoop_ClientSecret_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ClientSecret { get; set; }

        [LocalizedDisplayName(nameof(Resources.ParseDocumentWithHumanInTheLoop_FlowId_DisplayName))]
        [LocalizedDescription(nameof(Resources.ParseDocumentWithHumanInTheLoop_FlowId_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> FlowId { get; set; }

        [LocalizedDisplayName(nameof(Resources.ParseDocumentWithHumanInTheLoop_DocumentId_DisplayName))]
        [LocalizedDescription(nameof(Resources.ParseDocumentWithHumanInTheLoop_DocumentId_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> DocumentId { get; set; }

        [LocalizedDisplayName(nameof(Resources.ParseDocumentWithHumanInTheLoop_FilePath_DisplayName))]
        [LocalizedDescription(nameof(Resources.ParseDocumentWithHumanInTheLoop_FilePath_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> FilePath { get; set; }

        [LocalizedDisplayName(nameof(Resources.ParseDocumentWithHumanInTheLoop_Content_DisplayName))]
        [LocalizedDescription(nameof(Resources.ParseDocumentWithHumanInTheLoop_Content_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<byte[]> Content { get; set; }

        [LocalizedDisplayName(nameof(Resources.ParseDocumentWithHumanInTheLoop_Run_DisplayName))]
        [LocalizedDescription(nameof(Resources.ParseDocumentWithHumanInTheLoop_Run_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<JObject> Run { get; set; }

        #endregion




        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (ClientId == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ClientId)));
            if (ClientSecret == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ClientSecret)));
            if (FlowId == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(FlowId)));

            base.CacheMetadata(metadata);
        }

        protected override async Task<Action<AsyncCodeActivityContext>> ExecuteAsync(AsyncCodeActivityContext context, CancellationToken cancellationToken)
        {
            // Inputs
            var clientId = ClientId.Get(context);
            var clientSecret = ClientSecret.Get(context);
            var endpoint = "https://api.lucidtech.ai/v1";
            var authEndpoint = "auth.lucidtech.ai";
            var credentials = new Credentials(clientId, clientSecret, authEndpoint, endpoint);
            var client = new Client(credentials);

            var flowId = FlowId.Get(context);
            var documentId = DocumentId.Get(context);
            var content = Content.Get(context);
            var filePath = FilePath.Get(context);

            if (documentId == null && content != null) {
                var createDocumentResponse = (JObject)client.CreateDocument(content);
                documentId = (string)createDocumentResponse["documentId"];
            }
            else if(documentId == null && filePath != null) {
                content = File.ReadAllBytes(filePath);
                var createDocumentResponse = (JObject)client.CreateDocument(content);
                documentId = (string)createDocumentResponse["documentId"];
            }

            if (documentId == null) {
                throw new Exception("Either documentId, file or filePath must be specified");
            }
            
            var body = new Dictionary<string, object> { { "documentId", documentId } };
            var executionOutput= client.ExecuteWorkflow(flowId, body);

            // Outputs
            return (ctx) => {
                Run.Set(ctx, executionOutput);
            };
        }

        #endregion
    }
}


