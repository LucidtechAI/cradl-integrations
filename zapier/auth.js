const addAuthorization = async (request, z, bundle) => {
  parameters = [
    'grant_type=client_credentials',
    'audience=' + encodeURIComponent('https://api.cradl.ai/v1'),
    'client_id=' + encodeURIComponent(bundle.authData.client_id),
    'client_secret=' + encodeURIComponent(bundle.authData.client_secret),
  ]

  const authRequest = new Request(process.env.API_AUTH_URL, {
    method: 'POST',
    body: parameters.join('&'),
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    }
  })

  const response = await fetch(authRequest);
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