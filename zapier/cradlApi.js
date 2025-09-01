const http = require('https'); // require('http') if your URL is not https
CRADL_ORGANIZATION_ID = 'las:organization:cradl'


const downloadFile = async (url, z) => {
  const getResponse = await z.request({
    url: url,
    raw: true,
  })
  return getResponse
}

async function makePostRequest(z, endpoint, body) {
  return z.request({
    url: process.env.API_BASE_URL + endpoint,
    method: 'POST',
    body: body,
  });
}

async function makePatchRequest(z, endpoint, body) {
  return z.request({
    url: process.env.API_BASE_URL + endpoint,
    method: 'PATCH',
    body: body,
  });
}

async function makeGetRequest(z, endpoint) {
  return z.request({
    url: process.env.API_BASE_URL + endpoint,
    method: 'GET',
  });
}

async function putToFileServer(z, url, content) {
  return z.request({
    url: url,
    method: 'PUT',
    body: content,
    raw: true,
  });
}

async function getFromFileServer(z, url) {
  if (url == null) {
    return {}
  }
  
  return z.request({
    url: url,
    method: 'GET',
  });
}

async function getAction(z, actionId) {
  return makeGetRequest(z, '/actions/' + actionId)
}


async function listActions(z, nextToken) {
  return makeGetRequest(z, '/actions')
}

async function updateAction(z, actionId, body) {
  return makePatchRequest(z, '/actions/' + actionId, body)
}

async function listAgents(z) {
  return makeGetRequest(z, '/agents')
}

async function createAgentRun(z, agentId, variables) {
  return makePostRequest(z, '/agents/' + agentId + '/runs', {
    variables: variables,
  })
}

async function createDocument(z, inputFileUrl, agentRunId, fileName) {
  body = {}
  if (agentRunId !== undefined) {
    body.agentRunId = agentRunId
  }
  if (fileName !== undefined) {
    body.name = fileName
  }

  const postDocumentsResponse = await makePostRequest(z, '/documents', body)
  // bundle.inputData.file will be a URL from which we download the file
  fileResponse = await downloadFile(inputFileUrl, z)
  const fileServerResponse = await putToFileServer(z, postDocumentsResponse.json.fileUrl, fileResponse.buffer())
  return postDocumentsResponse.json.documentId
}

async function createPrediction(z, documentId, modelId){
  return makePostRequest(z, '/predictions', {
    documentId: documentId,
    modelId: modelId,
    postprocessConfig: {
      outputFormat: 'v2',
      parameters: {
        n: 3,
        collapse: true,
      },
      strategy: 'BEST_N_PAGES',
    }
  });
}

async function listModels(z) {
  return makeGetRequest(z, '/models?owner=me&owner=las:organization:cradl')
}

async function executeWorkflow(z, documentId, title, workflowId, metadata){
  input = {
    documentId: documentId,
    title: title,
  }
  if (metadata) {
    input.metadata = metadata
  }
  return makePostRequest(z, '/workflows/' + workflowId + '/executions', {input: input});
}

async function getWorkflow(z, workflowId) {
  return makeGetRequest(z, '/workflows/' + workflowId)
}

async function listWorkflows(z) {
  return makeGetRequest(z, '/workflows')
}

async function updateWorkflow(z, workflowId, metadata) {
  return makePatchRequest(z, '/workflows/' + workflowId, {
    metadata: metadata,
  })
}

async function getTransition(z, transitionId) {
  return makeGetRequest(z, '/transitions/' + transitionId)
}

async function updateTransition(z, transitionId, parameters) {
  return makePatchRequest(z, '/transitions/' + transitionId, {
    parameters: parameters,
  })
}

async function getSuccessfulAgentRuns(z, agentId) {
  return makeGetRequest(z, /agents/ + agentId + '/runs/?status=pending-export&status=review-completed&status=error&sort=createdTime%3Adesc&maxResults=20'
  )
}

async function getSuccessfulWorkflowExecutions(z, workflowId) {
  return makeGetRequest(z, /workflows/ + workflowId + '/executions/?status=succeeded&sortBy=startTime&order=descending&maxResults=3')
}

module.exports = {
  CRADL_ORGANIZATION_ID,
  createAgentRun,
  createDocument, 
  createPrediction,
  executeWorkflow,
  getAction,
  getFromFileServer,
  getSuccessfulAgentRuns,
  getSuccessfulWorkflowExecutions,
  getTransition,
  getWorkflow,
  listActions,
  listAgents,
  listModels,
  listWorkflows,
  makeGetRequest,
  updateAction,
  updateWorkflow,
  updateTransition,
}