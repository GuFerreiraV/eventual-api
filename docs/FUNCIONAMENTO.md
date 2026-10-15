## Pacotes & Dependências
```shell
dotnet add package Microsoft.AspNetCore.OpenApi 
Microsoft.EntityFrameworkCore.InMemory NSwag.AspNetCore

/* output
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.13" />
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.13" />
<PackageReference Include="NSwag.AspNetCore" Version="14.7.1" />
*/

```
## Desenvolvimento local
```shell
    dotnet dev-certs https --trust // confiança em certificado de desenvolvimento https
    dotnet run --launch-profile https // rodando app 
```

