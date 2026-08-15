# Guia do usuário do SocketSight

## Conteúdo

- [O que é esta ferramenta?](#o-que-é-esta-ferramenta)
- [Instalação e início rápido](#instalação-e-início-rápido)
- [Intervalo de verificação](#intervalo-de-verificação)
- [O que acontece após uma correspondência?](#o-que-acontece-após-uma-correspondência)
- [Como ver eventos](#como-ver-eventos)
- [Como interpretar um registro](#como-interpretar-um-registro)
- [Central de ajuda e atualizações](#central-de-ajuda-e-atualizações)
- [Registros, som e outras configurações](#registros-som-e-outras-configurações)
- [Privacidade, permissões e desinstalação](#privacidade-permissões-e-desinstalação)

## O que é esta ferramenta?

O SocketSight ajuda você a observar um endereço IP ou uma porta específica.

Quando uma conexão TCP corresponde a uma regra, o aplicativo registra horário, IP, porta e as informações de processo disponíveis no Windows, seguindo a ação de alerta escolhida.

Ele apenas observa, registra e alerta. Não fecha programas, altera o firewall nem bloqueia endereços IP.

## Instalação e início rápido

O idioma escolhido durante a instalação também é usado no aplicativo. Ao atualizar, escolher outro idioma altera o idioma do aplicativo uma vez; regras, configurações e registros permanecem.

Se a inicialização levar mais de cerca de 0,5 segundo, o SocketSight mostra uma tela breve que fecha quando a janela principal está pronta.

1. Abra **Regras de monitoramento**.
2. Selecione **Nova regra**.
3. Digite o IP ou a porta que deseja observar.
4. Salve e ative a regra.
5. Volte para **Início** e selecione **Iniciar monitoramento**.

Por exemplo, para observar `103.1.40.235:1433`:

- IP remoto: `103.1.40.235`
- Porta remota: `1433`
- Porta local: Qualquer
- Ação: alerta pop-up e registro
- Intervalo de repetição: 5 minutos

## Intervalo de verificação

O padrão é verificar a cada segundo. Em **Início**, você pode escolher de 0,5 a 10 segundos, em passos de 0,5.

Um intervalo menor detecta melhor conexões breves, mas usa mais recursos. Mesmo em 0,5 segundo, uma conexão que aparece e desaparece entre duas verificações pode não ser detectada.

Somente regras ativadas geram registros ou alertas.

## O que acontece após uma correspondência?

- **Registro silencioso:** escreve no registro sem alertar.
- **Aviso na bandeja e registro:** muda o ícone da bandeja para o estado de aviso; abrir o registro de eventos limpa o aviso.
- **Alerta pop-up e registro:** mostra uma janela na primeira correspondência; as seguintes atualizam a mesma janela.

Os números e formas na tela Início e na lista de eventos ajudam a distinguir as três ações.

## Como ver eventos

A mesma conexão aparece em um único registro, não como uma nova linha a cada segundo.

- Uma conexão existente aparece como **Ativa**.
- Uma conexão encerrada aparece como **Encerrada**.
- **Duração observada** é atualizada enquanto está ativa e para de mudar quando termina.
- **Aplicativo** mostra o nome do produto do arquivo quando disponível; caso contrário, mostra o nome do processo.
- Dê dois cliques em um registro para ver processo, PID, caminho, processos pai, serviços do Windows e outros detalhes. Também é possível copiar o registro.

Uma conexão é marcada como encerrada após ficar ausente da lista do Windows por dois segundos. Se voltar dentro desse período, continua sendo o mesmo registro; uma volta posterior cria um novo registro.

A duração começa quando o aplicativo vê a conexão pela primeira vez e pode não ser a duração real. O aplicativo não observa enquanto o monitoramento está parado; ao iniciar novamente, cria um novo registro.

## Como interpretar um registro

Uma correspondência significa apenas que apareceu uma conexão que você escolheu observar. Isso não prova a existência de malware.

Navegadores, proxies, VPNs ou componentes web podem já estar rodando em segundo plano. As informações do processo ajudam a identificar um aplicativo relacionado, mas não garantem qual aplicativo causou a conexão.

A lista de conexões TCP não mostra de forma confiável qual lado iniciou a conexão. As permissões do Windows também podem impedir a leitura de alguns caminhos, dados de arquivos, processos pai ou serviços.

Para avaliar um problema de segurança, combine essas informações com uma verificação antivírus ou orientação profissional.

## Central de ajuda e atualizações

Em **Configurações**, selecione **Abrir** ao lado da Central de ajuda para ler a visão geral do projeto e o guia do usuário. Os documentos seguem o idioma da interface.

Selecione **Verificar agora** para consultar no GitHub uma versão pública mais recente. O aplicativo não baixa, instala nem executa atualizações automaticamente.

Em **Configurações**, abra **Feedback** para escrever uma sugestão ou problema. O navegador abrirá um Issue do GitHub preenchido; revise o texto e envie você mesmo. Registros e conexões não são anexados por padrão.

## Registros, som e outras configurações

Os registros ficam em:

```text
%LOCALAPPDATA%\ConnectionWatcher\Logs\
```

O CSV é escrito quando uma conexão é encontrada e quando sua observação termina, não a cada segundo. O registro de eventos reúne a mesma conexão em uma única linha.

**Limpar exibição** oculta as linhas sem apagar os arquivos CSV. As linhas antigas continuam ocultas após reiniciar; eventos novos aparecem normalmente.

O limite padrão é 25 MB e pode ser alterado para 5–500 MB em **Configurações**. Até cinco arquivos são mantidos e o mais antigo é removido quando o limite é atingido.

**Abrir o aplicativo ao entrar no Windows** apenas abre o aplicativo. **Iniciar o monitoramento automaticamente ao abrir** começa a monitorar com as regras ativadas.

O som do alerta urgente é usado em alertas pop-up. Você pode ajustar o volume em **Configurações**; **Testar som** usa o mesmo volume e o volume do Windows também se aplica.

## Privacidade, permissões e desinstalação

- Não é necessária permissão de administrador, conta ou senha.
- O aplicativo não lê o conteúdo dos pacotes.
- Regras e registros não são enviados.
- O GitHub só é acessado ao verificar atualizações manualmente ou abrir a página de feedback.

Ao desinstalar, configurações e registros são mantidos por padrão. Se não precisar mais deles, exclua manualmente:

```text
%LOCALAPPDATA%\ConnectionWatcher
```
