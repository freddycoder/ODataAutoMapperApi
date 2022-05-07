# ODataAutomapperApi

This is a project to experiment with OData and Automapper with asp.net web api.

## Create a database migration

In the folder ODataAutomapperApi that contains the ODataAutomapperApi.csproj run this command:

```
dotnet ef --startup-project . migrations add InitialSchema --output-dir "Datas\\Migrations" --namespace "Datas.Migrations"
```

