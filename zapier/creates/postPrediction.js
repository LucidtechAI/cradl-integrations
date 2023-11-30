const cradlApi = require('../cradlApi')

const perform = async (z, bundle) => {
  const documentId = await cradlApi.createDocument(z, bundle.inputData.file)
  const createPredictionResponse = await cradlApi.createPrediction(z, documentId, bundle.inputData.modelId, bundle.inputData.trainingId)
  return createPredictionResponse.data;
};

const trainingField = async (z, bundle) => {
  if (bundle.inputData.modelId) {
    listModelsResponse = await cradlApi.listModels(z)
    models = listModelsResponse.data.models
    modelId = bundle.inputData.modelId.replace(cradlApi.CRADL_ORGANIZATION_ID + '/', '')
    model = models.find((m) => m.modelId == modelId)
    if (model.trainingId) {
      return [{ 
        key: 'trainingId', 
        required: false, 
        label: 'Training',
        dynamic: 'listTrainings.id.name',
      }] 
    }
  } 
  return []
}

module.exports = {
  key: 'postPrediction',
  noun: 'File',
  display: {
    label: 'Post Document to Model',
    description: 'Use a model to extract data from a document',
  },
  operation: {
    inputFields: [
      { 
        key: 'file', 
        required: true, 
        type: 'file', 
        label: 'Document',
      },
      { 
        key: 'modelId', 
        required: true, 
        label: 'Model',
        dynamic: 'listModels.id.name',
        altersDynamicFields: true,
      },
      trainingField,
    ],
    perform,
    sample: {
      file: 'SAMPLE FILE',
    },
  },
};
