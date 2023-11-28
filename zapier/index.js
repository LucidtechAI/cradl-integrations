const hydrators = require('./hydrators');
const listModels = require('./triggers/listModels');
const listTrainings = require('./triggers/listTrainings');
const listWorkflows = require('./triggers/listWorkflows');
const postPrediction = require('./creates/postPrediction');
const executeWorkflow = require('./creates/executeWorkflow');

const authentication = {
  type: 'custom',
  test: {
    url: process.env.API_BASE_URL + '/organizations/me',
  },
  // If you need any fields upfront, put them here
  fields: [
    { 
      key: 'app_client_id', 
      type: 'string', 
      required: true,
      helpText: 'App client ID'
    },
    { 
      key: 'app_client_secret', 
      type: 'password', 
      required: true,
      helpText: 'App client secret'
    },
  ],
  connectionLabel: '{{bundle.inputData.name}}', 
};

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
  // This is just shorthand to reference the installed dependencies you have.
  // Zapier will need to know these before we can upload.
  version: require('./package.json').version,
  platformVersion: require('zapier-platform-core').version,

  authentication,
  beforeRequest: [addAuthorization],
  // Any hydrators go here
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
