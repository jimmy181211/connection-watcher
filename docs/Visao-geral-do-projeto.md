# Monitor de conexões TCP

## Contexto e finalidade

Ao investigar uma conexão de rede incomum, muitas vezes precisamos responder a uma pergunta simples, mas difícil de confirmar a tempo:

> Meu computador se conectou a um endereço IP ou a uma porta específica? Se sim, quando aconteceu e qual programa criou a conexão?

O Monitor de Recursos do Windows mostra a atividade atual, mas o usuário precisa abri-lo e continuar observando. Uma conexão curta pode desaparecer rapidamente, e não é prático vigiar a janela por muito tempo. Ele também não avisa automaticamente sobre um destino escolhido nem mantém um histórico contínuo.

O Monitor de conexões TCP ajuda a resolver esse problema. Depois que o usuário escolhe um endereço IP ou uma porta, o aplicativo procura em segundo plano as conexões correspondentes. Ao encontrar uma, registra hora, endereços, portas e, quando disponíveis, programa e PID, depois avisa conforme as configurações.

Esta ferramenta não substitui o Monitor de Recursos nem um antivírus. Ela ajuda a observar destinos escolhidos, manter registros e fornecer informações para uma investigação de segurança posterior.

## Visão geral do projeto

O Monitor de conexões TCP é uma pequena **ferramenta do Windows para observar conexões de rede por meio de regras**. O usuário escolhe o IP remoto, a porta remota ou a porta local de interesse. Quando o Windows informa uma conexão TCP que corresponde a uma regra ativada, o aplicativo a registra ou mostra um aviso.

Em termos simples, ele observa um endereço IP ou uma porta específica. Por exemplo, pode monitorar `103.1.40.235:1433`. Quando surge uma conexão com esse destino, o aplicativo registra a hora, o status ativo ou encerrado, a duração observada, o programa e o PID. Dependendo da configuração, pode **registrar silenciosamente, mostrar um aviso na bandeja ou abrir uma janela de alerta.**

O aplicativo apenas informa: “Apareceu uma conexão que você decidiu observar.” Ele não classifica outras conexões como suspeitas, e uma conexão isolada não prova que o computador esteja infectado. Os dados podem ser compartilhados com uma equipe de segurança para investigação.

## Estrutura do projeto

```text
connection-watcher/
├── ConnectionWatcher.sln
├── src/
│   ├── ConnectionWatcher.Core/
│   └── ConnectionWatcher.App/
├── tests/
│   ├── ConnectionWatcher.Tests/
│   └── ConnectionWatcher.UiSmoke/
├── docs/
├── packaging/
└── Final-Share/
    ├── TCP-Connection-Watcher-Setup-win-x64.exe
    ├── SHA256SUMS.txt
    └── Docs/
```

- `ConnectionWatcher.sln`: arquivo de solução de todo o projeto.
- `src/ConnectionWatcher.Core`: lógica de configurações, regras, leitura de conexões TCP do Windows, remoção de duplicatas e registros CSV.
- `src/ConnectionWatcher.App`: interface do Windows em sete idiomas, com janela principal, editor de regras, central de ajuda, avisos e alertas.
- `tests`: testes funcionais e de interface; a suíte funcional tem atualmente 16 testes.
- `docs`: visões gerais e guias em sete idiomas.
- `packaging`: definições do instalador e notas da edição portátil.
- `Final-Share`: pasta final com um instalador multilíngue, documentos e somas SHA-256.

## Compilação e verificação

Para compilar no Windows é necessário o SDK do .NET 8.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Os pacotes publicados incluem `SHA256SUMS.txt`, que pode ser verificado com `Get-FileHash` no PowerShell.
