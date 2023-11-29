const auth = require('./auth');
const executeWorkflow = require('./creates/executeWorkflow');
const hydrators = require('./hydrators');
const listModels = require('./triggers/listModels');
const listTrainings = require('./triggers/listTrainings');
const listWorkflows = require('./triggers/listWorkflows');
const postPrediction = require('./creates/postPrediction');

module.exports = {
  version: require('./package.json').version,
  platformVersion: require('zapier-platform-core').version,
  
  authentication: auth.authentication,
  beforeRequest: [auth.addAuthorization],
  hydrators,

  triggers: {
    [listModels.key]: listModels,
    [listTrainings.key]: listTrainings,
    [listWorkflows.key]: listWorkflows,
  },

  creates: {
    [postPrediction.key]: postPrediction,
    [executeWorkflow.key]: executeWorkflow,
  },
};
