const cradlApi = require('../cradlApi')

const perform = async (z, bundle) => {
  const documentId = await cradlApi.createDocument(z, bundle.inputData.file)
  const executeWorkflowResponse = await cradlApi.executeWorkflow(z, documentId, bundle.inputData.workflowId)
  return executeWorkflowResponse.data;
};

module.exports = {
  key: 'executeWorkflow',
  noun: 'file',
  display: {
    label: 'executeWorkflow',
    description: 'Post a document to an existing workflow',
  },
  operation: {
    inputFields: [
      { 
        key: 'file', 
        required: true, 
        type: 'file', 
        label: 'File' 
      },
      { 
        key: 'workflowId', 
        required: true, 
        label: 'Workflow ID',
        dynamic: 'listWorkflows.id.name'
      },
    ],
    perform,
    sample: {
      file: 'SAMPLE FILE',
    },
  },
};
