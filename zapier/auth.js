const AUTH_SCOPES = [
  'api.lucidtech.ai/documents.write',
  'api.lucidtech.ai/models.read',
  'api.lucidtech.ai/organizations.read',
  'api.lucidtech.ai/predictions.write',
  'api.lucidtech.ai/trainings.read',
  'api.lucidtech.ai/transitions.read',
  'api.lucidtech.ai/transitions.write',
  'api.lucidtech.ai/workflows.executions.read',
  'api.lucidtech.ai/workflows.executions.write',
  'api.lucidtech.ai/workflows.read',
  'api.lucidtech.ai/workflows.write',
]

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
    body: {
      scope: AUTH_SCOPES,
    }
  });
  data = await response.json()
  request.headers.Authorization = `Bearer ${data.access_token}`;
  return request;
};

const authentication = {
  type: 'custom',
  test: {
    url: process.env.API_BASE_URL + '/organizations/me',
  },
  fields: [
    { 
      key: 'app_client_id', 
      type: 'string', 
      required: true,
      helpText: 'App client ID is found on the [App clients page](https://app.cradl.ai/appclients)'
    },
    { 
      key: 'app_client_secret', 
      type: 'password', 
      required: true,
      helpText: 'App client secret is found on the [App clients page](https://app.cradl.ai/appclients)'
    },
  ],
  connectionLabel: '{{bundle.inputData.name}}', 
};

module.exports={
  addAuthorization,
  authentication,
}