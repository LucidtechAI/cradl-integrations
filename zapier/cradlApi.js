const http = require('https'); // require('http') if your URL is not https

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
  // bundle.inputData.file will be a URL where the file data can be downloaded
  fileResponse = await downloadFile(inputFileUrl, z)
  const fileServerResponse = await putToFileServer(z, postDocumentsResponse.json.fileUrl, fileResponse.buffer())
  return postDocumentsResponse.json.documentId
}

async function createPrediction(z, documentId, modelId, trainingId){
  return makePostRequest(z, '/predictions', {
    documentId: documentId,
    modelId: modelId,
    trainingId: trainingId,
  });
}

async function executeWorkflow(z, documentId, workflowId){
  return makePostRequest(z, '/workflows/' + workflowId + '/executions', {
    input: {
      documentId: documentId,
    }
  });
}

async function listModels(z) {
    return makeGetRequest(z, '/models')
}

async function listTrainings(z, modelId) {
  return makeGetRequest(z, '/models/' + modelId + '/trainings')
}

async function listWorkflows(z) {
    return makeGetRequest(z, '/workflows')
}

module.exports = {
    createDocument, 
    createPrediction,
    executeWorkflow,
    listModels,
    listTrainings,
    listWorkflows,
}