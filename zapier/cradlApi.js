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

async function createDocument(z, inputFileUrl) {
  const postDocumentsResponse = await makePostRequest(z, '/documents', {})
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
      parameters: {
        n: 3,
      },
      strategy: 'BEST_N_PAGES',
      outputFormat: 'v2',
    }
  });
}

async function listModels(z) {
  return makeGetRequest(z, '/models?owner=me&owner=las:organization:cradl')
}

async function executeWorkflow(z, documentId, title, workflowId){
  return makePostRequest(z, '/workflows/' + workflowId + '/executions', {
    input: {
      documentId: documentId,
      title: title,
    }
  });
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

async function getSuccessfulWorkflowExecutions(z, workflowId) {
  return makeGetRequest(z, /workflows/ + workflowId + '/executions/?status=succeeded')
}


module.exports = {
  CRADL_ORGANIZATION_ID,
  createDocument, 
  createPrediction,
  executeWorkflow,
  getSuccessfulWorkflowExecutions,
  getTransition,
  getWorkflow,
  listModels,
  listTrainings,
  listWorkflows,
  updateWorkflow,
  updateTransition,
}