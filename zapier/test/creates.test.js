/* globals describe, expect, test */

const zapier = require('zapier-platform-core');

const App = require('../index');
const appTester = zapier.createAppTester(App);
zapier.tools.env.inject();


const TEST_FILE_URL = 'https://cdn.zapier.com/storage/files/f6679cf77afeaf6b8426de8d7b9642fc.pdf';


describe('creates', () => {
  test('postPrediction', async () => {
    const bundle = {
      inputData: {
        // in production, this will be a hydration URL to the selected file's data
        file: TEST_FILE_URL,
        modelId: process.env.TEST_MODEL_ID,
      },
      authData: {
        client_id: process.env.client_id,
        client_secret: process.env.client_secret,
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
        // in production, this will be a hydration URL to the selected file's data
        file: TEST_FILE_URL,
        fileName: 'test.pdf',
        workflowId: process.env.TEST_WORKFLOW_ID,
      },
      authData: {
        client_id: process.env.client_id,
        client_secret: process.env.client_secret,
      },
    };

    const result = await appTester(
      App.creates.executeWorkflow.operation.perform,
      bundle
    );
    expect(result.status).toBe('running');
  }, 60000);

  test('executeWorkflowWithMetadata', async () => {
    const bundle = {
      inputData: {
        // in production, this will be a hydration URL to the selected file's data
        file: TEST_FILE_URL,
        fileName: 'test.pdf',
        workflowId: process.env.TEST_WORKFLOW_ID,
        metadata: {testId: '123'}
      },
      authData: {
        client_id: process.env.client_id,
        client_secret: process.env.client_secret,
      },
    };

    const result = await appTester(
      App.creates.executeWorkflow.operation.perform,
      bundle
    );
    expect(result.status).toBe('running');
  }, 60000);
});
