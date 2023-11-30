const cradlApi = require('../cradlApi')

function pickIdAndName(training) {
  return {
    id: training.trainingId,
    name: training.name,
  }
}

const perform = async (z, bundle) => {
  if (bundle.inputData.modelId.startsWith(cradlApi.CRADL_ORGANIZATION_ID)) {
    // Pretrained models have no trainings and their trainings cannot be queried
    return []
  } else {
    // Get trainings for the model
    listTrainingsResponse = await cradlApi.listTrainings(z, bundle.inputData.modelId)
    trainings = listTrainingsResponse.data.trainings
    trainings = trainings.map(pickIdAndName)
  }
  return trainings
};

module.exports = {
  key: 'listTrainings',
  noun: 'Trainings',
  display: {
    label: 'listTrainings',
    description: 'Lists trainings',
    hidden: true,
  },
  operation: {
    perform,
  },
};
