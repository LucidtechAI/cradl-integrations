const http = require('https'); // require('http') if your URL is not https


const downloadFile = async (url, z) => {
  const getResponse = await z.request({
    url: url,
    raw: true,
  })
  return getResponse
}

async function makePostRequest(z, endpoint, body) {
  try {
    return await z.request({
      url: process.env.API_BASE_URL + endpoint,
      method: 'POST',
      body: body,
    });
  } catch (error) {
    console.log(error)
    return await z.request({
      url: process.env.BETA_API_BASE_URL + endpoint,
      method: 'POST',
      body: body,
    });
  }
  }

async function makePatchRequest(z, endpoint, body) {
  try {
    return await z.request({
      url: process.env.API_BASE_URL + endpoint,
      method: 'PATCH',
      body: body,
    });
  } catch (error) {
    console.log(error)
    return await z.request({
      url: process.env.BETA_API_BASE_URL + endpoint,
      method: 'PATCH',
      body: body,
    });
  }
}

async function makeGetRequest(z, endpoint) {
  try {
    return await z.request({
      url: process.env.API_BASE_URL + endpoint,
      method: 'GET',
    });
  } catch (error) {
    console.log(error)
    return await z.request({
      url: process.env.BETA_API_BASE_URL + endpoint,
      method: 'GET',
    });
  }
}

async function makeDeleteRequest(z, endpoint) {
  try {
    return await z.request({
      url: process.env.API_BASE_URL + endpoint,
      method: 'DELETE',
    });
  } catch (error) {
    console.log(error)
    return await z.request({
      url: process.env.BETA_API_BASE_URL + endpoint,
      method: 'DELETE',
    });
  }
}

async function putToFileServer(z, url, content) {
  return z.request({
    url: url,
    method: 'PUT',
    body: content,
    raw: true,
  });
}

async function getFromFileServer(z, url, raw=false) {
  if (url == null) {
    return {}
  }
  
  return z.request({
    url: url,
    method: 'GET',
    raw: raw,
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

async function getSuccessfulAgentRuns(z, agentId) {
  return makeGetRequest(z, /agents/ + agentId + '/runs/?status=pending-export&status=review-completed&status=error&status=completed&status=ready-for-review&sort=createdTime%3Adesc&maxResults=20'
  )
}

async function deleteAgentRun(z, agentId, runId) {
  return makeDeleteRequest(z, '/agents/' + agentId + '/runs/' + runId)
}

module.exports = {
  createAgentRun,
  createDocument, 
  deleteAgentRun,
  getAction,
  getFromFileServer,
  getSuccessfulAgentRuns,
  listActions,
  listAgents,
  makeGetRequest,
  updateAction,
}