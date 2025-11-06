const cradlApi = require('../cradlApi')
const hydrators = require('../hydrators');

const setWebhookUrl = async(z, bundle) => {
    body = {
      config: {
        url: bundle.targetUrl,
      },
    }

    await cradlApi.updateAction(z, bundle.inputData.actionId, body)

    return {
        actionId: bundle.inputData.actionId,
    };
  };
  
  const deleteWebhookUrl = async (z, bundle) => {
    body = {
      config: {
        url: null,
      },
    }

    updateActionResponse = cradlApi.updateAction(z, bundle.subscribeData.actionId, body)
    return updateActionResponse;
  };

  const processWebhookPayload = async(z, bundle) => {
    bundle.cleanedRequest = {
      ...bundle.cleanedRequest.context,
      ...bundle.cleanedRequest.output,
    }
    bundle.cleanedRequest = cleanPredictions(bundle.cleanedRequest)
    bundle.cleanedRequest.documentFileContent = z.dehydrateFile(hydrators.getDocument, {documentId: bundle.cleanedRequest.documentId})
    return [bundle.cleanedRequest];
  };

  const cleanPredictions = (d) => {
    for (let key in d) {
      data = d[key]
      if (data instanceof Array) {
        d[key] = data.map((item) => cleanPredictions(item))
      } else if ((data instanceof Object) && ('value' in data) && ('confidence' in data)) {
        d[key] = {
          value: data.value,
          name: data.name,
        }
      }
    }
    return d
  }

  const isExportable = (agentRun) => {
    if (
      agentRun.status === 'review-completed' ||
      agentRun.status === 'pending-export' ||
      agentRun.status === 'completed'
    ) {
      return true
    } 
    else if (
      agentRun.status === 'error' && 
      agentRun.events.some(
        event => event.errors.at(-1) === 'url must be defined and cannot be null'
      )
    ) {
      return true
    } 
    else 
      {
        return false
      }
  }

  const listCompleted = async (z, bundle) => {
    const getSuccessfulAgentRunsResponse = await cradlApi.getSuccessfulAgentRuns(z, bundle.inputData.agentId)

    outputs = getSuccessfulAgentRunsResponse.data.runs
    outputs = outputs.filter(
      agentRun => isExportable(agentRun)
    )

    outputs = Promise.all(outputs.map(async (run) => {
      fileServerResponse = await cradlApi.getFromFileServer(z, run.variablesFileUrl)
      output = fileServerResponse.data
      if (output === undefined) {
        run.output = {}
      }
      else {
        run.output = output
      }
      run.output = cleanPredictions(run.output)
      return run
    }))

    outputs = await outputs.map((run) => {
      documentIds = run.resourceIds.filter(
        resourceId => resourceId.startsWith('cradl:document')
      )
      if (documentIds) {
        documentId = documentIds[0]
        run.documentFileContent = z.dehydrateFile(hydrators.getDocument, {documentId: documentId})
      }
      else {
        run.documentFileContent = undefined
      }

      return run
    })

    outputs = outputs.map((run) => {
      return {
        id: run.id,
        documentFileContent: run.documentFileContent,
        ...run.output,
      };
    });

    
    return outputs
  };
  
  module.exports = {
    key: 'agentRunComplete',
    noun: 'Document',
    display: {
      label: 'Extracted Data From Document',
      description: 'Triggers when an Agent Run is completed.',
    },
    operation: {
      inputFields: [
        {
          key: 'agentId', 
          required: true, 
          label: 'Agent',
          dynamic: 'listAgents.id.name',
          helpText: 'The Agent you want to receive processed Documents from.',
          altersDynamicFields: true,
        },
        {
          key: 'actionId', 
          required: true, 
          label: 'Action',
          dynamic: 'listZapierActions.id.name',
          helpText: 'The Action that exports to this Zap.',
        },
      ],
      type: 'hook',
      sample: {
        id: 'sample_id'
      },
      performSubscribe: setWebhookUrl,
      performUnsubscribe: deleteWebhookUrl,
      perform: processWebhookPayload,
      performList: listCompleted,
    },
  };