const authentication = require('./authentication');
const executeWorkflow = require('./creates/executeWorkflow');
const hydrators = require('./hydrators');
const listModels = require('./triggers/listModels');
const listTrainings = require('./triggers/listTrainings');
const listWorkflows = require('./triggers/listWorkflows');
const postPrediction = require('./creates/postPrediction');

const addAuthorization = async (request, z, bundle) => {
  const basicHash = Buffer.from(
    `${bundle.authData.app_client_id}:${bundle.authData.app_client_secret}`
  ).toString(
    'base64'
  );
  const response = await fetch(process.env.API_AUTH_URL, {
    method: 'POST',
    headers: {
      'Authorization': `Basic ${basicHash}`,
      'Content-Type': 'application/x-www-form-urlencoded',
    },
  });
  data = await response.json()
  request.headers.Authorization = `Bearer ${data.access_token}`;
  return request;
};

module.exports = {
  version: require('./package.json').version,
  platformVersion: require('zapier-platform-core').version,

  authentication,
  beforeRequest: [addAuthorization],
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
