const cradlApi = require('../cradlApi')

const setWebhookUrl = async(z, bundle) => {
    const getWorkflowResponse = await cradlApi.getWorkflow(z, bundle.inputData.workflowId)
    metadata = getWorkflowResponse.data.metadata
    metadata.outputWebhookUri = bundle.targetUrl
    const updateWorkflowResponse = await cradlApi.updateWorkflow(z, bundle.inputData.workflowId, metadata)

    const getTransitionResponse = await cradlApi.getTransition(z, metadata.postprocessTransitionId)
    parameters = getTransitionResponse.data.parameters
    parameters.environment.WEBHOOK_URI = bundle.targetUrl
    const updateTransitionResponse = await cradlApi.updateTransition(z, metadata.postprocessTransitionId, parameters) 
    
    // Return data needed for deleting the webhook later.
    return {
        workflowId: bundle.inputData.workflowId,
        transitionId: metadata.postprocessTransitionId,
    };
  };
  
  const deleteWebhookUrl = async (z, bundle) => {
    const getWorkflowResponse = await cradlApi.getWorkflow(z, bundle.subscribeData.workflowId)
    metadata = getWorkflowResponse.data.metadata
    delete metadata.outputWebhookUri
    const updateWorkflowResponse = await cradlApi.updateWorkflow(z, bundle.subscribeData.workflowId, metadata)

    const getTransitionResponse = await cradlApi.getTransition(z, bundle.subscribeData.transitionId)
    parameters = getTransitionResponse.data.parameters
    delete parameters.environment.WEBHOOK_URI
    const updateTransitionResponse = await cradlApi.updateTransition(z, bundle.subscribeData.transitionId, parameters) 
    return updateTransitionResponse;
  };
  
  const processWebhookPayload = (z, bundle) => {
    return bundle.cleanedRequest;
  };
  
  const listCompleted = async (z, bundle) => {
    const getSuccessfulWorkflowExecutionsResponse = await cradlApi.getSuccessfulWorkflowExecutions(z, bundle.inputData.workflowId)
    console.log(getSuccessfulWorkflowExecutionsResponse)
    executions = getSuccessfulWorkflowExecutionsResponse.data.executions
    console.log(executions)
    outputs = executions.map((execution) => {return execution.output})
    return outputs
  };
  
  module.exports = {
    key: 'workflowComplete',
    noun: 'Item',
    display: {
      label: 'Workflow item completed',
      description: 'Trigger when a workflow item has been completed.',
    },
    operation: {
      inputFields: [
        {
          key: 'workflowId', 
          required: true, 
          label: 'Workflow',
          dynamic: 'listWorkflows.id.name',
          helpText: 'The workflow you want to connect.',
        },
      ],
      type: 'hook',
      performSubscribe: setWebhookUrl,
      performUnsubscribe: deleteWebhookUrl,
      perform: processWebhookPayload,
      performList: listCompleted,
    },
  };