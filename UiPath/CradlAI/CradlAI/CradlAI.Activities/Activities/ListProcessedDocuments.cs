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

namespace CradlAI.Activities
{
    [LocalizedDisplayName(nameof(Resources.ListProcessedDocuments_DisplayName))]
    [LocalizedDescription(nameof(Resources.ListProcessedDocuments_Description))]
    public class ListProcessedDocuments : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.ListProcessedDocuments_ClientId_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListProcessedDocuments_ClientId_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ClientId { get; set; }

        [LocalizedDisplayName(nameof(Resources.ListProcessedDocuments_ClientSecret_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListProcessedDocuments_ClientSecret_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ClientSecret { get; set; }

        [LocalizedDisplayName(nameof(Resources.ListProcessedDocuments_FlowId_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListProcessedDocuments_FlowId_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> FlowId { get; set; }

        [LocalizedDisplayName(nameof(Resources.ListProcessedDocuments_Status_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListProcessedDocuments_Status_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Status { get; set; }

        [LocalizedDisplayName(nameof(Resources.ListProcessedDocuments_Runs_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListProcessedDocuments_Runs_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<JObject> Runs { get; set; }

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
            var status = Status.Get(context);
            if (status == null) {
              status = "succeeded";
            }
            
            var response = client.ListWorkflowExecutions(flowId, status: status);

            // Outputs
            return (ctx) => {
                Runs.Set(ctx, response);
            };
        }

        #endregion
    }
}



