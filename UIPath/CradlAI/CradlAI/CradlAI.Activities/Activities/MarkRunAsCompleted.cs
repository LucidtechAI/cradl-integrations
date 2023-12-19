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
    [LocalizedDisplayName(nameof(Resources.MarkRunAsCompleted_DisplayName))]
    [LocalizedDescription(nameof(Resources.MarkRunAsCompleted_Description))]
    public class MarkRunAsCompleted : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.MarkRunAsCompleted_ClientId_DisplayName))]
        [LocalizedDescription(nameof(Resources.MarkRunAsCompleted_ClientId_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ClientId { get; set; }

        [LocalizedDisplayName(nameof(Resources.MarkRunAsCompleted_ClientSecret_DisplayName))]
        [LocalizedDescription(nameof(Resources.MarkRunAsCompleted_ClientSecret_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ClientSecret { get; set; }

        [LocalizedDisplayName(nameof(Resources.MarkRunAsCompleted_FlowId_DisplayName))]
        [LocalizedDescription(nameof(Resources.MarkRunAsCompleted_FlowId_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> FlowId { get; set; }

        [LocalizedDisplayName(nameof(Resources.MarkRunAsCompleted_RunId_DisplayName))]
        [LocalizedDescription(nameof(Resources.MarkRunAsCompleted_RunId_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> RunId { get; set; }

        [LocalizedDisplayName(nameof(Resources.MarkRunAsCompleted_Status_DisplayName))]
        [LocalizedDescription(nameof(Resources.MarkRunAsCompleted_Status_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Status { get; set; }

        [LocalizedDisplayName(nameof(Resources.MarkRunAsCompleted_Run_DisplayName))]
        [LocalizedDescription(nameof(Resources.MarkRunAsCompleted_Run_Description))]
        [LocalizedCategory(nameof(Resources.Output_Category))]
        public OutArgument<JObject> Run { get; set; }

        #endregion




        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (ClientId == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ClientId)));
            if (ClientSecret == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ClientSecret)));
            if (FlowId == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(FlowId)));
            if (RunId == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(RunId)));

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
            var runId = RunId.Get(context);
            var status = Status.Get(context);
            if (status == null) {
              status = "completed";
            }
            
            var response = client.UpdateWorkflowExecution(flowId, runId, status: status);

            // Outputs
            return (ctx) => {
                Run.Set(ctx, response);
            };
        }

        #endregion
    }
}




