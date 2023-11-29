module.exports={
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