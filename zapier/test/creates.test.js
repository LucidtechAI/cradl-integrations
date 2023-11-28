/* globals describe, expect, test */

const zapier = require('zapier-platform-core');

const App = require('../index');
const appTester = zapier.createAppTester(App);
zapier.tools.env.inject();


const FILE_URL =
  'https://cdn.zapier.com/storage/files/f6679cf77afeaf6b8426de8d7b9642fc.pdf';


describe('creates', () => {
  test('postPrediction', async () => {
    const bundle = {
      inputData: {
        // in production, this will be an hydration URL to the selected file's data
        file: FILE_URL,
        modelId: process.env.TEST_MODEL_ID,
      },
      authData: {
        app_client_id: process.env.app_client_id,
        app_client_secret: process.env.app_client_secret,
      }
    };

    const result = await appTester(
      App.creates.postPrediction.operation.perform,
      bundle
    );
    expect(result.predictions);
    expect(result.predictionId);
  }, 60000);

  test('executeWorkflow', async () => {
    const bundle = {
      inputData: {
        // in production, this will be an hydration URL to the selected file's data
        file: FILE_URL,
        workflowId: process.env.TEST_WORKFLOW_ID,
      },
      authData: {
        app_client_id: process.env.app_client_id,
        app_client_secret: process.env.app_client_secret,
      },
    };

    const result = await appTester(
      App.creates.executeWorkflow.operation.perform,
      bundle
    );
    expect(result.status).toBe('running');
  }, 60000);
});
