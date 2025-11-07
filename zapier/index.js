const auth = require('./auth');
const agentRunComplete = require('./triggers/agentRunComplete');
const createAgentRun = require('./creates/createAgentRun');
const hydrators = require('./hydrators');
const listAgents = require('./triggers/listAgents');
const listZapierActions = require('./triggers/listZapierActions');

module.exports = {
  version: require('./package.json').version,
  platformVersion: require('zapier-platform-core').version,
  
  authentication: auth.authentication,
  beforeRequest: [auth.addAuthorization],
  hydrators,
  flags: {
    cleanInputData: false,  // global flag
  },

  triggers: {
    [agentRunComplete.key]: agentRunComplete,
    [listAgents.key]: listAgents,
    [listZapierActions.key]: listZapierActions,
  },

  creates: {
    [createAgentRun.key]: createAgentRun,
  },
};
