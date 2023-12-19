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


namespace CradlAI.Activities
{
    [LocalizedDisplayName(nameof(Resources.ListFlows_DisplayName))]
    [LocalizedDescription(nameof(Resources.ListFlows_Description))]
    public class ListFlows : ContinuableAsyncCodeActivity
    {
        #region Properties

        /// <summary>
        /// If set, continue executing the remaining activities even if the current activity has failed.
        /// </summary>
        [LocalizedCategory(nameof(Resources.Common_Category))]
        [LocalizedDisplayName(nameof(Resources.ContinueOnError_DisplayName))]
        [LocalizedDescription(nameof(Resources.ContinueOnError_Description))]
        public override InArgument<bool> ContinueOnError { get; set; }

        [LocalizedDisplayName(nameof(Resources.ListFlows_ClientId_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListFlows_ClientId_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ClientId { get; set; }

        [LocalizedDisplayName(nameof(Resources.ListFlows_ClientSecret_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListFlows_ClientSecret_Description))]
        [LocalizedCategory(nameof(Resources.Authentication_Category))]
        public InArgument<string> ClientSecret { get; set; }

        [LocalizedDisplayName(nameof(Resources.ListFlows_Endpoint_DisplayName))]
        [LocalizedDescription(nameof(Resources.ListFlows_Endpoint_Description))]
        [LocalizedCategory(nameof(Resources.Input_Category))]
        public InArgument<string> Endpoint { get; set; }

        #endregion


        #region Constructors

        public ListFlows()
        {
        }

        #endregion


        #region Protected Methods

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            if (ClientId == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ClientId)));
            if (ClientSecret == null) metadata.AddValidationError(string.Format(Resources.ValidationValue_Error, nameof(ClientSecret)));

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
           
            var workflows = client.ListWorkflows(1, null);
            var res = JsonSerialPublisher.ObjectToDict<Dictionary<string, object>>(workflows);
            // Outputs
            return (ctx) => {
           //     Flows.Set(ctx, workflows);
            };

        }

        #endregion
    }
}

