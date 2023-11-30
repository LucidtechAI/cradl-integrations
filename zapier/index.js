const auth = require('./auth');
const executeWorkflow = require('./creates/executeWorkflow');
const hydrators = require('./hydrators');
const listModels = require('./triggers/listModels');
const listWorkflows = require('./triggers/listWorkflows');
const postPrediction = require('./creates/postPrediction');
const workflowComplete = require('./triggers/workflowComplete')

module.exports = {
  version: require('./package.json').version,
  platformVersion: require('zapier-platform-core').version,
  
  authentication: auth.authentication,
  beforeRequest: [auth.addAuthorization],
  hydrators,

  triggers: {
    [listModels.key]: listModels,
    [listWorkflows.key]: listWorkflows,
    [workflowComplete.key]: workflowComplete,
  },

  creates: {
    [postPrediction.key]: postPrediction,
    [executeWorkflow.key]: executeWorkflow,
  },
};
