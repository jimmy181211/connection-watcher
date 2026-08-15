# Monitor de conexões TCP

## Contexto e finalidade

Ao investigar uma conexão de rede incomum, muitas vezes precisamos responder a uma pergunta simples, mas difícil de confirmar a tempo:

> Meu computador se conectou a um endereço IP ou a uma porta específica? Se sim, quando aconteceu, a qual processo o Windows associou a conexão e qual contexto do aplicativo pode ser obtido?

O Monitor de Recursos do Windows mostra a atividade atual, mas o usuário precisa abri-lo e continuar observando. Uma conexão curta pode desaparecer rapidamente, e não é prático vigiar a janela por muito tempo. Ele também não avisa automaticamente sobre um destino escolhido nem mantém um histórico contínuo.

O Monitor de conexões TCP ajuda a resolver esse problema. Depois que o usuário escolhe um endereço IP ou uma porta, o aplicativo procura em segundo plano as conexões correspondentes. Ao encontrar uma, registra hora, endereços, portas, o proprietário da conexão informado pelo Windows e, quando disponíveis, informações do arquivo, processos superiores e serviços do Windows, depois avisa conforme as configurações.

Esta ferramenta não substitui o Monitor de Recursos nem um antivírus. Ela ajuda a observar destinos escolhidos, manter registros e fornecer informações para uma investigação de segurança posterior.

## Visão geral do projeto

O Monitor de conexões TCP é uma pequena **ferramenta do Windows para observar conexões de rede por meio de regras**. O usuário escolhe o IP remoto, a porta remota ou a porta local de interesse. Quando o Windows informa uma conexão TCP que corresponde a uma regra ativada, o aplicativo a registra ou mostra um aviso.

Em termos simples, ele observa um endereço IP ou uma porta específica. Por exemplo, pode monitorar `103.1.40.235:1433`. Quando surge uma conexão com esse destino, o aplicativo registra a hora, o status ativo ou encerrado, a duração observada, o proprietário informado pelo Windows, o PID e o contexto disponível do aplicativo. Dependendo da configuração, pode **registrar silenciosamente, mostrar um aviso na bandeja ou abrir uma janela de alerta.**

O intervalo padrão de verificação é de um segundo. O usuário pode escolher de 0,5 a 10 segundos, em passos de 0,5 segundo. Um intervalo menor tem mais chance de detectar conexões breves; um intervalo maior usa menos recursos, mas pode não detectá-las.

O aplicativo apenas informa: “Apareceu uma conexão que você decidiu observar.” Ele não classifica outras conexões como suspeitas, e uma conexão isolada não prova que o computador esteja infectado. Os dados podem ser compartilhados com uma equipe de segurança para investigação.

## Estrutura do projeto

```text
connection-watcher/
├── ConnectionWatcher.sln
├── RELEASE_NOTES.md
├── src/
│   ├── ConnectionWatcher.Core/
│   └── ConnectionWatcher.App/
├── tests/
│   ├── ConnectionWatcher.Tests/
│   └── ConnectionWatcher.UiSmoke/
├── docs/
├── learning/
├── scripts/
│   └── build-release.ps1
├── packaging/
└── Final-Share/
    ├── TCP-Connection-Watcher-Setup-win-x64.exe
    ├── SHA256SUMS.txt
    └── Docs/
```

- `ConnectionWatcher.sln`: arquivo de solução de todo o projeto.
- `src/ConnectionWatcher.Core`: lógica de configurações, regras, leitura de conexões TCP do Windows, acompanhamento temporal, contexto de processos e registros CSV compatíveis com versões anteriores.
- `src/ConnectionWatcher.App`: interface do Windows em sete idiomas, com janela principal, editor de regras, detalhes de eventos, central de ajuda, verificação de atualizações, avisos e alertas.
- `tests`: 20 testes funcionais e de compatibilidade, além de testes de interface multilíngue e escala de DPI.
- `docs`: visões gerais e guias em sete idiomas.
- `learning`: tutorial para desenvolvedores e material de estudo da arquitetura.
- `scripts/build-release.ps1`: executa as verificações e gera automaticamente `artifacts`, `dist` e `Final-Share`, nessa ordem.
- `packaging`: definições do instalador e notas da edição portátil.
- `Final-Share`: pasta local ignorada pelo Git, com um instalador multilíngue, os sete conjuntos de documentos, notas da versão e somas SHA-256.

## Compilação e verificação

Para compilar no Windows é necessário o SDK do .NET 8.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Os pacotes publicados incluem `SHA256SUMS.txt`, que pode ser verificado com `Get-FileHash` no PowerShell.

Os mantenedores podem executar `scripts\build-release.ps1` para compilar, testar, publicar, empacotar, copiar os documentos atuais e gerar as somas em um único fluxo.
