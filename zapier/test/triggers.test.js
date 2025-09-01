const zapier = require('zapier-platform-core');

const App = require('../index');
const appTester = zapier.createAppTester(App);
zapier.tools.env.inject();

describe('triggers', () => {
  test('listAgents', async () => {
    const bundle = {
      authData: {
        client_id: process.env.client_id_v2,
        client_secret: process.env.client_secret_v2,
      },
    };
    const results = await appTester(
      App.triggers.listAgents.operation.perform,
      bundle,
    );
    expect(results.length).toBeGreaterThan(0);
  });

  test('listModels', async () => {
    const bundle = {
      authData: {
        client_id: process.env.client_id,
        client_secret: process.env.client_secret,
      },
    };
    const results = await appTester(
      App.triggers.listModels.operation.perform,
      bundle,
    );

    expect(results.length).toBeGreaterThan(0);
  });

  test('listWorkflows', async () => {
    const bundle = {
      authData: {
        client_id: process.env.client_id,
        client_secret: process.env.client_secret,
      },
    };
    const results = await appTester(
      App.triggers.listWorkflows.operation.perform,
      bundle,
    );

    expect(results.length).toBeGreaterThan(0);
  });

  test('listZapierActions', async () => {
    const bundle = {
      inputData: {
        agentId: process.env.TEST_AGENT_ID,
      },
      authData: {
        client_id: process.env.client_id_v2,
        client_secret: process.env.client_secret_v2,
      },
    };
    const results = await appTester(
      App.triggers.listZapierActions.operation.perform,
      bundle,
    );
    expect(results.length).toBeGreaterThan(0);
  });

  test('agentRunComplete', async () => {
    const bundle = {
      authData: {
        client_id: process.env.client_id_v2,
        client_secret: process.env.client_secret_v2,
      },
      targetUrl: 'https://example.com',
      inputData: {
        actionId: process.env.TEST_ACTION_ID,
        agentId: process.env.TEST_AGENT_ID,
      }
    };
    const subscribeResults = await appTester(
      App.triggers.agentRunComplete.operation.performSubscribe,
      bundle,
    )

    bundle.subscribeData = subscribeResults

    const unsubscribeResults = await appTester(
       App.triggers.agentRunComplete.operation.performUnsubscribe,
       bundle,
     )

    const performListResults = await appTester(
      App.triggers.agentRunComplete.operation.performList,
      bundle,
    )
    console.log(performListResults)
  }, 60000)

  test('workflowComplete', async () => {
    const bundle = {
      authData: {
        client_id: process.env.client_id,
        client_secret: process.env.client_secret,
      },
      targetUrl: 'https://example.com',
      inputData: {
        workflowId: process.env.TEST_WORKFLOW_ID,
      }
    };
    const subscribeResults = await appTester(
      App.triggers.workflowComplete.operation.performSubscribe,
      bundle,
    )

    bundle.subscribeData = subscribeResults

    const unsubscribeResults = await appTester(
      App.triggers.workflowComplete.operation.performUnsubscribe,
      bundle,
    )

    const performListResults = await appTester(
      App.triggers.workflowComplete.operation.performList,
      bundle,
    )
  }, 60000)
});