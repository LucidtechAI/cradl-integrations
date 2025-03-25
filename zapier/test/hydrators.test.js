/* globals describe, expect, test */

const zapier = require('zapier-platform-core');

const App = require('../index');
const appTester = zapier.createAppTester(App);
zapier.tools.env.inject();

describe('downloadFile', () => {
  test('download file', async () => {
    if (!process.env.ZAPIER_DEPLOY_KEY) {
      console.warn('skipped as ZAPIER_DEPLOY_KEY is not defined');
      return;
    }

    const bundle = {
      inputData: {
        url: 'https://httpbin.zapier-tooling.com/xml',
      },
      authData: {
        client_id: process.env.client_id,
        client_secret: process.env.client_secret,
      },
    };

    const url = await appTester(App.hydrators.downloadFile, bundle);
    expect(url).toContain(
      'https://zapier-dev-files.s3.amazonaws.com/cli-platform/'
    );
  });

  test('get document', async () => {
    const bundle = {
      inputData: {
        documentId: process.env.TEST_DOCUMENT_ID,
      },
      authData: {
        client_id: process.env.client_id,
        client_secret: process.env.client_secret,
      },
    };

    const url = await appTester(App.hydrators.getDocument, bundle);
    expect(url).toContain(
      'https://zapier-dev-files.s3.amazonaws.com/cli-platform/'
    );
  });
});
