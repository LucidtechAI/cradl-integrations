const cradlApi = require('../cradlApi')

function pickIdAndName(agent) {
  return {
    id: agent.agentId,
    name: agent.name,
  }
}

const perform = async (z, bundle) => {
  listAgentsResponse = await cradlApi.listAgents(z, bundle.inputData.modelId)
  agents = listAgentsResponse.data.agents
  agents.sort((a, b) => Date.parse(a.updatedTime) - Date.parse(b.updatedTime)).reverse()
  agents = agents.map(pickIdAndName)
  return agents
};

module.exports = {
  key: 'listAgents',
  noun: 'Agents',
  display: {
    label: 'listAgents',
    description: 'Lists agents',
    hidden: true,
  },
  operation: {
    perform,
  },
};
