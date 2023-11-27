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
    label: 'postPrediction',
    description: 'Post a prediction to an existing model',
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
