# Guia do usuário do Monitor de conexões TCP

## Finalidade principal

Esta ferramenta ajuda você a **observar um endereço IP ou uma porta escolhida**. Ela pode:

- Registrar automaticamente quando uma conexão aparece
- Registrar endereços IP e portas locais e remotas
- Registrar, quando disponíveis, o proprietário da conexão informado pelo Windows, PID, caminho do executável, informações do arquivo, processos superiores ou hospedeiros e serviços do Windows relacionados
- Registrar silenciosamente, mostrar um aviso na bandeja ou abrir uma janela de alerta
- Guardar informações para consulta posterior ou para compartilhar com profissionais de segurança
- Confirmar se uma nova conexão com o mesmo destino aparece mais tarde

## Como funciona

Primeiro crie uma regra para indicar o endereço IP ou a porta a observar. Depois ative a regra e inicie o monitoramento. Por padrão, o aplicativo verifica a lista de conexões TCP do Windows uma vez por segundo. Na página **Início**, você pode ajustar o intervalo de 0,5 a 10 segundos, em passos de 0,5 segundo. Intervalos menores têm mais chance de detectar conexões breves; intervalos maiores usam menos recursos, mas podem não detectá-las. Apenas conexões que correspondem a uma regra ativada são processadas; as demais não geram registros nem avisos.

Quando uma conexão corresponde a uma regra, o aplicativo executa a ação escolhida:

- **Registrar silenciosamente:** grava o evento no CSV sem mudar o ícone da bandeja nem mostrar contador.
- **Aviso na bandeja e registro:** não abre uma janela. O ícone muda para o estado de aviso, que é limpo ao abrir o Registro de eventos.
- **Alerta em janela e registro:** abre uma janela assim que surge a primeira correspondência. Enquanto ela estiver aberta, outras correspondências atualizam a mesma janela. Depois de fechada, o intervalo da regra define quando outro alerta pode aparecer.

A página Início mostra um símbolo compacto para cada ação. **Regras de monitoramento** combina o símbolo com um nome curto; a coluna **Ação** do Registro de eventos mostra somente o símbolo:

- `1 ●` círculo cinza: Registrar silenciosamente
- `2 ▲` triângulo laranja: Aviso na bandeja e registro
- `3 ◆` losango vermelho: Alerta em janela e registro

O número e o formato também distinguem as ações sem depender da cor. Aponte para um símbolo para ver o nome completo.

#### *Observação importante:*

1. Uma correspondência significa apenas que apareceu uma conexão escolhida. Isso não prova que o computador esteja infectado.
2. Esta ferramenta **somente registra conexões e mostra avisos**. Outras decisões de segurança também devem considerar uma verificação antivírus e a orientação de profissionais qualificados.

## Primeiro uso

1. Escolha um dos sete idiomas durante a instalação; a edição portátil também pergunta o idioma na primeira abertura.
2. Abra **Regras de monitoramento**.
3. Selecione **Nova regra**.
4. Digite as condições nos campos do formulário.
5. Confira a visualização da regra na parte inferior.
6. Salve e ative a regra.
7. Volte para **Início** e selecione **Iniciar monitoramento**.

### Exemplo

Para observar se qualquer porta local volta a se conectar a `103.1.40.235:1433`, crie esta regra:

- Tipo: Conexão TCP
- IP remoto: `103.1.40.235`
- Porta remota: `1433`
- Porta local: Qualquer
- Ação: Alerta em janela e registro
- Intervalo de repetição: 5 minutos

## Registros

Os registros são armazenados em:

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

Cada nova conexão correspondente aparece como uma única linha no **Registro de eventos**. Se permanecer aberta por várias horas, não será registrada novamente a cada segundo. **Status** mostra se está ativa ou encerrada, e **Duração observada** é atualizada enquanto está ativa e fica fixa depois que termina.

Para facilitar a leitura, a tabela mostra apenas os campos principais. A coluna **Aplicativo** usa as informações disponíveis do produto do arquivo e, caso não existam, mostra o nome do processo. Clique duas vezes em uma linha para abrir **Detalhes do evento** e ver o proprietário da conexão informado pelo Windows, PID, caminho, informações do produto, até três processos superiores ou hospedeiros, serviços do Windows relacionados e os demais campos da conexão. O status ativo e a duração continuam sendo atualizados, e **Copiar detalhes** copia o registro completo.

Esse contexto pode ajudar a identificar qual aplicativo está relacionado a uma conexão, mas nem sempre prova qual aplicativo a provocou. Por exemplo, um navegador, proxy, VPN ou componente web integrado pode já estar em execução em segundo plano.

A duração observada começa quando o aplicativo vê a conexão pela primeira vez, por isso pode ser menor que a duração real. Depois que o monitoramento é interrompido, o aplicativo não sabe se a conexão terminou nesse intervalo; iniciar novamente cria uma nova observação. O CSV interno escreve informações apenas na detecção e no encerramento, e o aplicativo combina esses dados em uma linha.

Uma conexão só é marcada como encerrada depois de ficar ausente da tabela de conexões do Windows por dois segundos. Se reaparecer durante esse período de tolerância, continua sendo a mesma observação. O horário de encerramento corresponde ao último momento em que o aplicativo realmente viu a conexão. Um novo aparecimento depois desse período cria outro registro.

Selecione **Limpar exibição** quando quiser deixar o Registro de eventos organizado. Isso oculta as linhas existentes da interface sem excluir os registros CSV. Os eventos anteriores continuam ocultos depois que o aplicativo é reiniciado, enquanto novos eventos aparecem normalmente.

O limite total padrão é de 25 MB e pode ser alterado para 5–500 MB em **Configurações**. O aplicativo usa até cinco arquivos e remove os registros mais antigos ao atingir o limite.

## Central de ajuda

Em **Configurações**, selecione **Abrir central de ajuda** para ler a visão geral do projeto e o guia no idioma atual da interface.

## Atualizações do software

Em **Configurações**, selecione **Verificar agora** para consultar no GitHub a versão pública mais recente. O aplicativo faz isso apenas quando você solicita. Se houver uma versão mais nova, você poderá abrir a página do GitHub Release, ler as notas e fazer o download por conta própria. O aplicativo não baixa, instala nem executa atualizações automaticamente e não envia regras ou registros.

## Inicialização e som de alerta

- **Abrir o aplicativo ao entrar no Windows:** abre o aplicativo após o login, mas não inicia o monitoramento.
- **Iniciar o monitoramento automaticamente ao abrir:** inicia com as regras ativadas.
- **Som do alerta urgente:** usa um breve som integrado e não depende do esquema de sons do Windows. Defina o volume entre 10% e 100% (40% por padrão). **Testar som** aparece ao lado do controle de volume; o teste e os alertas urgentes reais usam o mesmo nível, e o volume do Windows também continua valendo.

## Limitações importantes

1. A verificação ocorre uma vez por segundo por padrão. Mesmo com o ajuste de 0,5 segundo, uma conexão que aparece e desaparece entre duas verificações pode não ser detectada.
2. A versão 1 **monitora apenas TCP**, não UDP.
3. A tabela TCP do Windows não informa de forma totalmente confiável qual lado iniciou a conexão.
4. As permissões do Windows ou o encerramento rápido de um processo podem impedir a leitura do caminho, das informações do arquivo, do processo superior ou do serviço relacionado. O PID e qualquer nome de processo disponível continuam sendo registrados. O contexto de processos e serviços é uma evidência para investigação, não uma conclusão garantida sobre a causa principal.
5. Não há monitoramento quando o aplicativo está fechado, parado ou quando o computador está suspenso.
6. A duração observada começa na primeira detecção. Sua precisão depende do intervalo escolhido; não é uma hora exata de início fornecida pelo Windows.
7. O aplicativo não fecha programas, não altera o firewall nem bloqueia endereços IP.

## Privacidade e permissões

1. Não são necessários direitos de administrador.
2. Não são necessários login, nome de usuário, senha ou e-mail.
3. O aplicativo só se conecta ao GitHub depois que você seleciona manualmente **Verificar agora**. Ele não se conecta a um servidor do desenvolvedor nem envia regras ou registros.
4. Ele não lê o conteúdo dos pacotes.
5. As configurações ficam em `%LOCALAPPDATA%\ConnectionWatcher\config.json`.

## Desinstalação

Você pode remover a versão instalada em **Aplicativos instalados** no Windows. A desinstalação remove o programa, mas mantém por padrão as configurações e os registros em `%LOCALAPPDATA%\ConnectionWatcher`, evitando perda acidental. Exclua essa pasta manualmente quando tiver certeza de que não precisa mais dos dados.
