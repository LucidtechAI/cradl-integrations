const cradlApi = require('../cradlApi')

const perform = async (z, bundle) => {
  const documentId = await cradlApi.createDocument(z, bundle.inputData.file)
  const executeWorkflowResponse = await cradlApi.executeWorkflow(z, documentId, bundle.inputData.fileName, bundle.inputData.workflowId)
  return executeWorkflowResponse.data;
};

module.exports = {
  key: 'executeWorkflow',
  noun: 'File',
  display: {
    label: 'Upload Document to Flow',
    description: 'Post a document to an existing workflow for human-in-the-loop processing',
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
        key: 'fileName', 
        type: 'string', 
        label: 'Filename' 
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
