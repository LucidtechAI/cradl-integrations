const auth = require('./auth');
const agentRunComplete = require('./triggers/agentRunComplete');
const createAgentRun = require('./creates/createAgentRun');
const executeWorkflow = require('./creates/executeWorkflow');
const hydrators = require('./hydrators');
const listAgents = require('./triggers/listAgents');
const listModels = require('./triggers/listModels');
const listWorkflows = require('./triggers/listWorkflows');
const listZapierActions = require('./triggers/listZapierActions');
const postPrediction = require('./creates/postPrediction');
const workflowComplete = require('./triggers/workflowComplete');

module.exports = {
  version: require('./package.json').version,
  platformVersion: require('zapier-platform-core').version,
  
  authentication: auth.authentication,
  beforeRequest: [auth.addAuthorization],
  hydrators,

  triggers: {
    [agentRunComplete.key]: agentRunComplete,
    [listAgents.key]: listAgents,
    [listModels.key]: listModels,
    [listWorkflows.key]: listWorkflows,
    [listZapierActions.key]: listZapierActions,
    [workflowComplete.key]: workflowComplete,
  },

  creates: {
    [createAgentRun.key]: createAgentRun,
    [postPrediction.key]: postPrediction,
    [executeWorkflow.key]: executeWorkflow,
  },
};
