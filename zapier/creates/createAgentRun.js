const cradlApi = require('../cradlApi')

const perform = async (z, bundle) => {
    if (bundle.inputData.variables === undefined) {
      bundle.inputData.variables = {}
    }
    bundle.inputData.variables.fileName = bundle.inputData.fileName
    bundle.inputData.variables.source = 'zapier'
    const createAgentRunResponse = await cradlApi.createAgentRun(z, bundle.inputData.agentId, bundle.inputData.variables)
    const agentRunId = createAgentRunResponse.json.agentId + '/' + createAgentRunResponse.json.runId
    await cradlApi.createDocument(z, bundle.inputData.file, agentRunId, bundle.inputData.fileName)
  return createAgentRunResponse.data;
};

module.exports = {
  key: 'createAgentRun',
  noun: 'Document',
  display: {
    label: 'Extract Data From Document',
    description: 'Send a Document to an existing Agent for processing.',
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
        key: 'agentId', 
        required: true, 
        label: 'Agent',
        dynamic: 'listAgents.id.name',
      },
      {
        key: 'variables',
        label: 'Variables',
        dict: true,
      },
    ],
    perform,
    sample: {
      file: 'SAMPLE FILE',
    },
  },
};
