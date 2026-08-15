# Visão geral do projeto SocketSight

## Conteúdo

- [Contexto e objetivo](#contexto-e-objetivo)
- [Visão geral do projeto](#visão-geral-do-projeto)
- [Principais decisões de design](#principais-decisões-de-design)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Inicialização, idioma e central de ajuda](#inicialização-idioma-e-central-de-ajuda)
- [Compilação e verificação](#compilação-e-verificação)

## Contexto e objetivo

O Monitor de Recursos do Windows mostra a atividade de rede atual, mas o usuário precisa mantê-lo aberto e observá-lo. Uma conexão rápida pode desaparecer antes de ser percebida, e não é prático guardar um histórico de um alvo específico.

O SocketSight permite criar regras para um IP remoto, uma porta remota ou uma porta local. Ele processa apenas conexões TCP correspondentes e registra horário, estado, duração observada, o processo informado pelo Windows e o contexto do aplicativo disponível.

Ele não substitui o Monitor de Recursos nem um antivírus. Seu objetivo é facilitar a observação repetida de uma conexão escolhida e sua análise posterior pelo usuário ou por um profissional de segurança.

## Visão geral do projeto

O SocketSight é uma ferramenta local de observação de conexões TCP para Windows, baseada em regras. Depois que o monitoramento é iniciado, ele lê a tabela de conexões TCP do Windows no intervalo escolhido e processa as conexões que correspondem às regras ativadas.

O intervalo padrão é de um segundo. É possível escolher de 0,5 a 10 segundos, em passos de 0,5 segundo. Intervalos menores detectam melhor conexões breves, mas fazem mais verificações; intervalos maiores usam menos recursos, mas podem perder conexões breves.

O aplicativo só registra ou alerta sobre conexões selecionadas pelas regras. Ele não classifica automaticamente outras atividades de rede como suspeitas. Esta versão concentra-se em TCP; UDP exigiria outro projeto de rastreamento de baixo nível e uma atribuição de aplicativo mais complexa.

## Principais decisões de design

- **Regras primeiro:** somente conexões que correspondem a regras ativadas são processadas.
- **Uma observação por conexão:** uma conexão contínua não é gravada novamente a cada segundo.
- **Encerramento por tempo real:** ela termina após ficar ausente por dois segundos; se voltar nesse período, continua sendo a mesma observação.
- **Contexto do aplicativo é uma pista:** informações de processo, PID, arquivo, processo pai e serviço do Windows ajudam na investigação, mas não provam a causa final.
- **Visualização e dados separados:** **Limpar exibição** oculta linhas antigas sem apagar os arquivos CSV.
- **Execução local:** o aplicativo não lê o conteúdo dos pacotes nem envia regras e registros. Ele acessa o GitHub somente quando o usuário verifica atualizações ou abre a página de feedback.

## Estrutura do projeto

```text
connection-watcher/
├── ConnectionWatcher.sln
├── RELEASE_NOTES.md
├── src/
│   ├── ConnectionWatcher.Core/       # regras, monitoramento, estado, registros e configurações
│   └── ConnectionWatcher.App/        # interface WinForms, idiomas, bandeja e inicialização
├── tests/
│   ├── ConnectionWatcher.Tests/      # testes principais e de compatibilidade
│   └── ConnectionWatcher.UiSmoke/    # testes de idioma, DPI e layout
├── docs/                             # visões gerais e guias do usuário
├── learning/                         # tutorial e material de aprendizagem
├── scripts/build-release.ps1         # compilação, testes, pacote e preparação da versão
├── packaging/                        # definição do instalador Inno Setup
└── Final-Share/                      # arquivos finais para os usuários
```

- `ConnectionWatcher.Core` contém regras, leitura TCP do Windows, rastreamento de conexões, contexto de processos, registros CSV e configurações.
- `ConnectionWatcher.App` contém a interface, o editor de regras, detalhes de eventos, central de ajuda, avisos, alertas, idiomas e tela de inicialização.
- `tests` protege o comportamento principal e verifica diferentes idiomas e escalas de exibição.
- `scripts` compila, testa, publica o aplicativo independente, cria o instalador, copia os documentos atuais e gera somas SHA-256.
- `artifacts` é a saída de publicação, `dist` é a saída do instalador e `Final-Share` é o pacote final para usuários. Todos podem ser recriados.

O usuário baixa um único instalador: `SocketSight-Setup-win-x64.exe`. O aplicativo instalado é independente e usa vários arquivos; não é necessário instalar o runtime do .NET separadamente.

## Inicialização, idioma e central de ajuda

O instalador oferece sete idiomas. O idioma escolhido durante a instalação também se torna o idioma da interface do SocketSight. Ao atualizar, um novo idioma substitui o anterior uma vez; regras, configurações e registros são preservados.

Se a inicialização levar mais de cerca de 0,5 segundo, o SocketSight mostra uma tela local breve. As mensagens são apenas indicações de status; não significam conexão com a Internet nem uma verificação adicional. A tela fecha quando a janela principal está pronta.

A central de ajuda em Configurações mostra a visão geral do projeto e o guia do usuário no idioma atual. A verificação de atualizações é manual; o aplicativo não baixa, instala nem executa atualizações automaticamente.

## Compilação e verificação

Para compilar no Windows, são necessários o .NET 8 SDK e o Inno Setup.

```powershell
dotnet build ConnectionWatcher.sln --configuration Release
dotnet run --project tests\ConnectionWatcher.Tests\ConnectionWatcher.Tests.csproj --configuration Release
```

Os mantenedores podem executar:

```powershell
scripts\build-release.ps1
```

O script compila, testa, publica, cria o instalador, reúne os documentos atuais e gera somas SHA-256. Os destinatários podem usar `Get-FileHash` no PowerShell para verificar o instalador.
