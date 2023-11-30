const cradlApi = require('../cradlApi')

function pickIdAndName(model) {
  const modelId = (model.organizationId == cradlApi.CRADL_ORGANIZATION_ID) ? cradlApi.CRADL_ORGANIZATION_ID + '/' + model.modelId : model.modelId
  return {
    id: modelId,
    name: model.name,
  }
}

const perform = async (z, bundle) => {
  listModelsResponse = await cradlApi.listModels(z)
  models = listModelsResponse.data.models
  models.sort((a, b) => Date.parse(a.updatedTime) - Date.parse(b.updatedTime)).reverse()
  models = models.map(pickIdAndName)
  return models
};

module.exports = {
  key: 'listModels',
  noun: 'Models',
  display: {
    label: 'listModels',
    description: 'Lists models',
    hidden: true,
  },
  operation: {
    perform,
  },
};
