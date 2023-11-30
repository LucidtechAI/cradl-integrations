const cradlApi = require('../cradlApi')

const perform = async (z, bundle) => {
  const documentId = await cradlApi.createDocument(z, bundle.inputData.file)
  const createPredictionResponse = await cradlApi.createPrediction(z, documentId, bundle.inputData.modelId)
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
        label: 'Document',
      },
      { 
        key: 'modelId', 
        required: true, 
        label: 'Model',
        dynamic: 'listModels.id.name',
        altersDynamicFields: true,
      },
    ],
    perform,
    sample: {
      file: 'SAMPLE FILE',
    },
  },
};
