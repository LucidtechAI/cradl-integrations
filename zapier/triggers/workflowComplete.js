const cradlApi = require('../cradlApi')

const setWebhookUrl = async(z, bundle) => {
    const getWorkflowResponse = await cradlApi.getWorkflow(z, bundle.inputData.workflowId)
    metadata = getWorkflowResponse.data.metadata
    metadata.outputWebhookUri = bundle.targetUrl
    cradlApi.updateWorkflow(z, bundle.inputData.workflowId, metadata)

    const getTransitionResponse = await cradlApi.getTransition(z, metadata.postprocessTransitionId)
    parameters = getTransitionResponse.data.parameters
    parameters.environment.WEBHOOK_URI = bundle.targetUrl
    cradlApi.updateTransition(z, metadata.postprocessTransitionId, parameters) 
    
    // Return data needed for deleting the webhook later.
    return {
        workflowId: bundle.inputData.workflowId,
        transitionId: metadata.postprocessTransitionId,
    };
  };
  
  const deleteWebhookUrl = async (z, bundle) => {
    cradlApi.updateWorkflow(z, bundle.subscribeData.workflowId, {outputWebhookUri: null})

    const getTransitionResponse = await cradlApi.getTransition(z, bundle.subscribeData.transitionId)
    parameters = getTransitionResponse.data.parameters
    const updateTransitionResponse = await cradlApi.updateTransition(z, bundle.subscribeData.transitionId, parameters) 
    return updateTransitionResponse;
  };
  
  const processWebhookPayload = (z, bundle) => {
    return [bundle.cleanedRequest];
  };
  
  const listCompleted = async (z, bundle) => {
    const getSuccessfulWorkflowExecutionsResponse = await cradlApi.getSuccessfulWorkflowExecutions(z, bundle.inputData.workflowId)
    executions = getSuccessfulWorkflowExecutionsResponse.data.executions
    outputs = executions.map((execution) => {return execution.output})
    return outputs
  };
  
  module.exports = {
    key: 'workflowComplete',
    noun: 'Document',
    display: {
      label: 'Document Parsing Completed',
      description: 'Triggers when a document processing flow has has completed.',
    },
    operation: {
      inputFields: [
        {
          key: 'workflowId', 
          required: true, 
          label: 'Flow',
          dynamic: 'listWorkflows.id.name',
          helpText: 'The flow you want to receive processed documents from.',
        },
      ],
      type: 'hook',
      sample: {
        datasetId: 'las:dataset:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx',
        documentId: 'las:document:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx',
        values: {
          field_1: 'ACME INC',
          field_2: '1234.00',
          field_3: '2020-01-01',
          line_items_field: [
            {
              line_items_field_1: 'LINE 1 DESCRIPTION',
              line_items_field_2: '616.00',
            },
            {
              line_items_field_1: 'LINE 2 DESCRIPTION',
              line_items_field_2: '618.00',
            },
          ]
        }
      },
      performSubscribe: setWebhookUrl,
      performUnsubscribe: deleteWebhookUrl,
      perform: processWebhookPayload,
      performList: listCompleted,
    },
  };