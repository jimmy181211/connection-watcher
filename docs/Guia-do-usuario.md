# Guia do usuário do Monitor de conexões TCP

## Finalidade principal

Esta ferramenta ajuda você a **observar um endereço IP ou uma porta escolhida**. Ela pode:

- Registrar automaticamente quando uma conexão aparece
- Registrar endereços IP e portas locais e remotas
- Registrar programa, PID e caminho do executável quando disponíveis
- Registrar silenciosamente, mostrar um aviso na bandeja ou abrir uma janela de alerta
- Guardar informações para consulta posterior ou para compartilhar com profissionais de segurança
- Confirmar se uma nova conexão com o mesmo destino aparece mais tarde

## Como funciona

Primeiro crie uma regra para indicar o endereço IP ou a porta a observar. Depois ative a regra e inicie o monitoramento. O aplicativo verifica a lista de conexões TCP do Windows uma vez por segundo. Apenas conexões que correspondem a uma regra ativada são processadas; as demais não geram registros nem avisos.

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

Para facilitar a leitura, a tabela mostra apenas os campos principais. Clique duas vezes em uma linha para abrir **Detalhes do evento** e ver as regras, o destino local, o estado TCP, o PID, o caminho do programa e a ação. O status ativo e a duração continuam sendo atualizados, e **Copiar detalhes** copia o registro completo.

A duração observada começa quando o aplicativo vê a conexão pela primeira vez, por isso pode ser menor que a duração real. Depois que o monitoramento é interrompido, o aplicativo não sabe se a conexão terminou nesse intervalo; iniciar novamente cria uma nova observação. O CSV interno escreve informações apenas na detecção e no encerramento, e o aplicativo combina esses dados em uma linha.

Também é criado um novo registro quando uma conexão desaparece durante duas verificações e volta a aparecer.

O limite total padrão é de 25 MB e pode ser alterado para 5–500 MB em **Configurações**. O aplicativo usa até cinco arquivos e remove os registros mais antigos ao atingir o limite.

## Central de ajuda

Em **Configurações**, selecione **Abrir central de ajuda** para ler a visão geral do projeto e o guia no idioma atual da interface.

## Inicialização e som de alerta

- **Abrir o aplicativo ao entrar no Windows:** abre o aplicativo após o login, mas não inicia o monitoramento.
- **Iniciar o monitoramento automaticamente ao abrir:** inicia com as regras ativadas.
- **Som do alerta urgente:** usa um breve som integrado e não depende do esquema de sons do Windows. Defina o volume entre 10% e 100% (40% por padrão). O teste e os alertas reais usam o mesmo valor; o volume do Windows também continua valendo.

## Limitações importantes

1. A verificação ocorre uma vez por segundo; uma conexão com menos de um segundo pode não ser detectada.
2. A versão 1 **monitora apenas TCP**, não UDP.
3. A tabela TCP do Windows não informa de forma totalmente confiável qual lado iniciou a conexão.
4. As permissões do Windows podem impedir a leitura do caminho de processos do sistema ou protegidos; o PID e qualquer nome disponível continuam sendo registrados.
5. Não há monitoramento quando o aplicativo está fechado, parado ou quando o computador está suspenso.
6. A duração observada começa na primeira detecção e tem precisão aproximada de um segundo; não é uma hora exata de início fornecida pelo Windows.
7. O aplicativo não fecha programas, não altera o firewall nem bloqueia endereços IP.

## Privacidade e permissões

1. Não são necessários direitos de administrador.
2. Não são necessários login, nome de usuário, senha ou e-mail.
3. O aplicativo não se conecta a um servidor do desenvolvedor nem envia registros.
4. Ele não lê o conteúdo dos pacotes.
5. As configurações ficam em `%LOCALAPPDATA%\ConnectionWatcher\config.json`.

## Desinstalação

Você pode remover a versão instalada em **Aplicativos instalados** no Windows. A desinstalação remove o programa, mas mantém por padrão as configurações e os registros em `%LOCALAPPDATA%\ConnectionWatcher`, evitando perda acidental. Exclua essa pasta manualmente quando tiver certeza de que não precisa mais dos dados.
