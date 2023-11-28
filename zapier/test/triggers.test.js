const zapier = require('zapier-platform-core');

const App = require('../index');
const appTester = zapier.createAppTester(App);
zapier.tools.env.inject();

describe('triggers', () => {
  test('listModels', async () => {
    const bundle = {
      authData: {
        app_client_id: process.env.app_client_id,
        app_client_secret: process.env.app_client_secret,
      },
    };
    const results = await appTester(
      App.triggers.listModels.operation.perform,
      bundle
    );

    expect(results.length).toBeGreaterThan(0);
  });

  test('listTrainings', async () => {
    const bundle = {
      inputData: {
        modelId: process.env.TEST_MODEL_ID,
      },
      authData: {
        app_client_id: process.env.app_client_id,
        app_client_secret: process.env.app_client_secret,
      },
    };
    const results = await appTester(
      App.triggers.listTrainings.operation.perform,
      bundle
    );
  });


  test('listWorkflows', async () => {
    const bundle = {
      authData: {
        app_client_id: process.env.app_client_id,
        app_client_secret: process.env.app_client_secret,
      },
    };
    const results = await appTester(
      App.triggers.listWorkflows.operation.perform,
      bundle
    );

    expect(results.length).toBeGreaterThan(0);
  });
});