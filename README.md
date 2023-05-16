# managed identity for Azure Functions (.NET)

Sample C# codes to authenticate and authorize managed identity of Azure functions.

## func-cs01

- Caller application (HTTP trigger) to call func-cs02.
- audienceId (application Id for func-cs02) and targetUrl (API endpoint for func-cs02) should be specified.

## func-cs02

- Callee application (HTTP trigger) to be called by func-cs01.

## Usage

- Create function apps in Azure, and deployed func-cs01 and func-cs02 to Azure.
- System assigned managed identity is enabled on func-cs01 function app.
- Authentication is enabled on func-cs02 function app.

