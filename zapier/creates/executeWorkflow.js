const cradlApi = require('../cradlApi')

const perform = async (z, bundle) => {
  const documentId = await cradlApi.createDocument(z, bundle.inputData.file)
  const executeWorkflowResponse = await cradlApi.executeWorkflow(z, documentId, bundle.inputData.fileName, bundle.inputData.workflowId)
  return executeWorkflowResponse.data;
};

module.exports = {
  key: 'executeWorkflow',
  noun: 'Document',
  display: {
    label: 'Parse Document with Human-in-the-Loop',
    description: 'Send a document to an existing flow for human-in-the-loop processing.',
  },
  operation: {
    inputFields: [
      { 
        key: 'file', 
        required: true, 
        type: 'file', 
        label: 'Document' ,
      },
      { 
        key: 'fileName', 
        type: 'string', 
        label: 'Filename' ,
      },
      { 
        key: 'workflowId', 
        required: true, 
        label: 'Flow',
        dynamic: 'listWorkflows.id.name',
      },
    ],
    perform,
    sample: {
      file: 'SAMPLE FILE',
    },
  },
};
