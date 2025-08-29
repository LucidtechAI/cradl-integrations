const cradlApi = require('../cradlApi')

function pickIdAndName(action) {
  return {
    id: action.actionId,
    name: action.name,
  }
}

const perform = async (z, bundle) => {
  listActionsResponse = await cradlApi.listActions(z)
  actions = listActionsResponse.data.actions
  while (listActionsResponse.data.nextToken) {
    listActionsResponse = await cradlApi.listActions(z, nextToken=listActionsResponse.data.nextToken)
    actions = actions.push(...listActionsResponse.data.actions)
  }
  actions = actions.filter(action => action.agentId === bundle.inputData.agentId)
  actions = actions.filter(action => action.functionId === 'cradl:organization:cradl/cradl:function:export-to-zapier')
  actions = actions.map(pickIdAndName)
  return actions
};

module.exports = {
  key: 'listZapierActions',
  noun: 'Actions',
  display: {
    label: 'listZapierActions',
    description: 'Lists actions',
    hidden: true,
  },
  operation: {
    perform,
  },
};
