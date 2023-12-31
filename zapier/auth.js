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
    `${bundle.authData.client_id}:${bundle.authData.client_secret}`
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
  if (response.status === 400) {
    throw new z.errors.Error(
      // This message is surfaced to the user
      'Your Client ID and/or Client secret are incorrect. Find credentials in the Cradl app by going to https://app.cradl.ai/flows, selecting your Flow and adding Zapier as output under "Export".',
      'AuthenticationError',
      response.status
    );
  }
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
      key: 'client_id', 
      type: 'string', 
      required: true,
      helpText: 'Client ID is found on the [App clients page](https://app.cradl.ai/flows) after selecting your Flow and adding Zapier as output under "Export".'
    },
    { 
      key: 'client_secret', 
      type: 'password', 
      required: true,
      helpText: 'Client secret is found on the [App clients page](https://app.cradl.ai/flows) after selecting your Flow and adding Zapier as output under "Export".'
    },
  ],
  connectionLabel: '{{bundle.inputData.name}}', 
};

module.exports={
  addAuthorization,
  authentication,
}