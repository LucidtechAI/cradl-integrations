const cradlApi = require('../cradlApi')

const perform = async (z, bundle) => {
  const documentId = await cradlApi.createDocument(z, bundle.inputData.file)
  const createPredictionResponse = await cradlApi.createPrediction(z, documentId, bundle.inputData.modelId, bundle.inputData.trainingId)
  return createPredictionResponse.data;
};

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
        label: 'File',
      },
      { 
        key: 'modelId', 
        required: true, 
        label: 'Model ID',
        dynamic: 'listModels.id.name',
      },
      { 
        key: 'trainingId', 
        required: false, 
        label: 'Training ID',
        dynamic: 'listTrainings.id.name',
      },
    ],
    perform,
    sample: {
      file: 'SAMPLE FILE',
    },
  },
};
