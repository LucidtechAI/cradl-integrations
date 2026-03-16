const addAuthorization = async (request, z, bundle) => {
  const parameters = [
    'grant_type=client_credentials',
    'audience=' + encodeURIComponent('https://api.cradl.ai/v1'),
    'client_id=' + encodeURIComponent(bundle.authData.client_id),
    'client_secret=' + encodeURIComponent(bundle.authData.client_secret),
  ]

  const data = {
    method: 'POST',
    body: parameters.join('&'),
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    }
  }

  const authRequest = new Request(process.env.API_AUTH_URL, data)
  response = await fetch(authRequest);
  if (response.status === 400) {
    // Try the BETA API 
    const authRequest = new Request(process.env.BETA_API_AUTH_URL, data)
    response = await fetch(authRequest);
    if (response.status === 400) {
      // Credentials don't work with PROD or BETA, so throw
      throw new z.errors.Error(
        // This message is surfaced to the user
        'Your Client ID and/or Client Secret are incorrect. Find credentials in the Cradl app here: https://rc.app.cradl.ai/settings/api',
        'AuthenticationError',
        response.status
      );
    }
  }

  auth_data = await response.json()
  request.headers.Authorization = `Bearer ${auth_data.access_token}`;
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
      helpText: 'Client ID is found under [API settings](https://rc.app.cradl.ai/settings/api).'
    },
    { 
      key: 'client_secret', 
      type: 'password', 
      required: true,
      helpText: 'Client Secret is found under [API settings](https://rc.app.cradl.ai/settings/api).'
    },
  ],
  connectionLabel: '{{bundle.inputData.name}}', 
};

module.exports={
  addAuthorization,
  authentication,
}