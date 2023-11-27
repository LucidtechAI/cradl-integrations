const cradlApi = require('../cradlApi')

function pickIdAndName(training) {
  return {
    id: training.trainingId,
    name: training.name,
  }
}

const perform = async (z, bundle) => {
  listTrainingsResponse = await cradlApi.listTrainings(z, bundle.inputData.modelId)
  trainings = listTrainingsResponse.data.trainings
  trainings = trainings.map(pickIdAndName)
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
