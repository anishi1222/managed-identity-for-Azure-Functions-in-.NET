# managed identity for Azure Functions (.NET)

Sample C# codes to authenticate and authorize managed identity of Azure functions.

## Runtime

- Targets .NET 10 on Azure Functions v4 using the isolated worker model.
- Set `FUNCTIONS_WORKER_RUNTIME` to `dotnet-isolated` for both function apps.
- For Azure deployment, set the function app stack to .NET isolated 10.0. On Windows, use `netFrameworkVersion` `v10.0`; on Linux, use `linuxFxVersion` `DOTNET-ISOLATED|10.0` and a plan that supports .NET 10, such as Flex Consumption.

## func-cs01

- Caller application (HTTP trigger) to call func-cs02.
- audienceId (application Id for func-cs02) and targetUrl (API endpoint for func-cs02) should be specified.

## func-cs02

- Callee application (HTTP trigger) to be called by func-cs01.

## Usage

- Create function apps in Azure, and deployed func-cs01 and func-cs02 to Azure.
- System assigned managed identity is enabled on func-cs01 function app.
- Authentication is enabled on func-cs02 function app.

