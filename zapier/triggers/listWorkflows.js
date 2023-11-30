const cradlApi = require('../cradlApi')

function pickIdAndName(workflow) {
  return {
    id: workflow.workflowId,
    name: workflow.name,
  }
}

const perform = async (z, bundle) => {
  listWorkflowsResponse = await cradlApi.listWorkflows(z, bundle.inputData.modelId)
  workflows = listWorkflowsResponse.data.workflows
  workflows.sort((a, b) => Date.parse(a.updatedTime) - Date.parse(b.updatedTime)).reverse()
  workflows = workflows.map(pickIdAndName)
  return workflows
};

module.exports = {
  key: 'listWorkflows',
  noun: 'Workflows',
  display: {
    label: 'listWorkflows',
    description: 'Lists workflows',
    hidden: true,
  },
  operation: {
    perform,
  },
};
