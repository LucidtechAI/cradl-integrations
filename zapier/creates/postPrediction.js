const cradlApi = require('../cradlApi')


function cleanItem(item) {
  isLine = false
  for ([key, value] of Object.entries(item[0])) {
    if (typeof(value) == 'object' && key != 'location') {
      isLine = true
    }
  }
  if (isLine) {
    for ([idx, line] of item.entries()) {
      item[idx] = cleanPrediction(line)
    }
  } else {
    item = item[0]
  }
  return item
}

function cleanPrediction(a) {
  for ([key, value] of Object.entries(a)) {
    a[key] = cleanItem(value)
  }
  return a
}

const perform = async (z, bundle) => {
  const documentId = await cradlApi.createDocument(z, bundle.inputData.file)
  const createPredictionResponse = await cradlApi.createPrediction(z, documentId, bundle.inputData.modelId)
  output = createPredictionResponse.data
  output.predictions = cleanPrediction(output.predictions)
  return output
};

module.exports = {
  key: 'postPrediction',
  noun: 'File',
  display: {
    label: 'Parse Document',
    description: 'Use a model to parse data from a document.',
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
