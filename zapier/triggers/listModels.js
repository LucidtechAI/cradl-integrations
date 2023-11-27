const cradlApi = require('../cradlApi')

function pickIdAndName(model) {
  return {
    id: model.modelId,
    name: model.name,
  }
}

const perform = async (z, bundle) => {
  listModelsResponse = await cradlApi.listModels(z)
  models = listModelsResponse.data.models
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
