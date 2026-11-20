# Aplicativo de realidade aumentada para Android

Projeto desenvolvido em Unity para um teste prático de desenvolvimento em realidade aumentada (AR). O objetivo é demonstrar a integração de um SDK de AR, a organização da lógica do aplicativo e a execução em um dispositivo físico.

O aplicativo reconhece uma imagem-alvo e permite escolher entre quatro personagens: Pikachu, Charmander, Squirtle e Bulbasaur. O personagem aparece na posição correspondente ao centro do QR code presente na imagem e reage ao toque com uma animação. Também é possível substituir o personagem pelo menu.

**O reconhecimento utiliza a imagem completa como referência visual. O conteúdo do QR code não é lido nem interpretado pelo aplicativo.** Essa abordagem corresponde à opção de usar uma imagem-alvo prevista no teste.

## Downloads e execução no Android

O APK e a imagem para reconhecimento estão disponíveis na [página de releases](https://github.com/superjojodev/AR-basic-app-project/releases). A imagem também está no repositório: [marker-pokeball.png](Assets/Markers/marker-pokeball.png).

### Requisitos

- Dispositivo Android compatível com o Google ARCore.
- Android 8.0 (API 26) ou superior, conforme o mínimo configurado no projeto. A versão do Android, por si só, não garante compatibilidade com ARCore.
- Arquitetura ARM64, utilizada no build configurado.
- Permissão de acesso à câmera e serviços de AR necessários disponíveis no aparelho.
- Imagem-alvo impressa ou exibida em outra tela.

### Como testar

1. Baixe o APK na página de releases e instale-o no dispositivo. Caso o Android solicite, autorize a instalação pela origem utilizada para abrir o arquivo.
2. Imprima ou exiba a imagem-alvo completa com **7 × 7 cm**, dimensão física configurada na biblioteca de imagens. Não recorte apenas o QR code.
3. Mantenha o marcador parado, em um local iluminado e sem reflexos que dificultem a visualização.
4. Abra o aplicativo e conceda acesso à câmera.
5. Aponte a câmera para a imagem. Quando ela for reconhecida, aparecerão um indicador ciano de posicionamento e o botão **Choose character**.
6. Abra o menu e escolha um personagem. Ele será criado na posição indicada; o indicador será ocultado.
7. Mova o dispositivo para observar o personagem e toque nele para acionar sua reação. Aguarde o término da reação antes de tocar novamente.
8. Abra o menu para escolher outro personagem. A nova escolha substitui o personagem anterior. Selecionar o mesmo personagem apenas fecha o menu.

Se o personagem desaparecer após uma perda de rastreamento, aponte novamente a câmera para a imagem. Veja as condições de rastreamento em [Limitações conhecidas](#limitações-conhecidas).

## Atendimento aos objetivos do teste

| Objetivo ou requisito | Solução e situação atual |
| --- | --- |
| Pesquisar e integrar um SDK de AR | Uso do AR Foundation com o ARCore XR Plugin, aproveitando a estrutura inicial do template AR Mobile. A justificativa está na seção de tecnologias. |
| Detectar um QR code ou uma imagem-alvo | O `ARTrackedImageManager` reconhece a imagem cadastrada na biblioteca. O reconhecimento mostra a prévia de posicionamento e habilita a seleção. |
| Posicionar um objeto 3D no ambiente real | Após a seleção, o personagem é criado em um ponto definido do marcador. Sua posição acompanha a pose fornecida pelo rastreamento da imagem, em vez de acompanhar a câmera. |
| Ter uma animação contínua | Os quatro personagens reproduzem continuamente, em loop, a animação do estado `idle` dos Animator Controllers. Esse comportamento foi validado durante os testes, atendendo ao requisito. |
| Ter uma animação reativa | O toque no personagem aciona o parâmetro `React` do Animator Controller. Novos acionamentos são bloqueados enquanto a reação está em andamento. |
| Exibir um botão de ação sobre a câmera | O botão de seleção permite criar ou trocar o personagem, mantendo uma única instância por vez. |
| Exibir um indicador de estado | Há uma prévia visual de posicionamento e controles condicionados ao rastreamento. Ainda falta um indicador explícito de estados, como “procurando marcador” e “objeto ativo”; esse requisito não é considerado integralmente concluído nesta documentação. |
| Organizar o código de forma justificável | Rastreamento e posicionamento, menu e reação ao toque estão separados em três scripts principais. |
| Executar em um dispositivo físico | O aplicativo foi testado em Android durante o desenvolvimento, validando o funcionamento em dispositivo físico. O modelo do aparelho e a versão do sistema ainda precisam ser registrados para tornar a validação reproduzível. |
| Diferencial opcional: som reativo | Os prefabs possuem uma fonte de áudio associada à lógica que reproduz som na criação e no início da reação ao toque. |

A criação do personagem depende da escolha do usuário após a detecção. O reconhecimento do marcador, sozinho, não cria automaticamente um personagem.

## Tecnologias e decisões

| Tecnologia | Versão | Uso |
| --- | --- | --- |
| Unity | 6000.6.9f1 | Editor e engine do aplicativo. |
| AR Foundation | 6.6.2 | Componentes de rastreamento de imagens e gerenciamento da sessão de AR. |
| ARCore XR Plugin | 6.6.2 | Integração com os recursos de AR no Android. |
| XR Interaction Toolkit | 3.6.1 | Base das interações de seleção e toque. |
| Unity glTFast | 6.21.0 | Importação de modelos GLB, incluindo o modelo provisório usado na validação inicial. |

As versões estão registradas em `ProjectSettings/ProjectVersion.txt` e `Packages/manifest.json`. Python foi utilizado fora do Unity para gerar o QR code do marcador; não é necessário para executar o aplicativo.

O **AR Foundation** foi escolhido por sua integração com o Unity e por já estar configurado no template AR Mobile utilizado como base. Isso permitiu concentrar o desenvolvimento na lógica específica do projeto. No Android, o **ARCore** fornece os recursos de AR por meio do ARCore XR Plugin.

A escolha de uma **imagem-alvo** atende ao enunciado sem exigir a implementação de um leitor de conteúdo de QR codes. A imagem completa é cadastrada na biblioteca, e um ponto normalizado dentro dela define a posição do personagem.

O posicionamento foi mantido independente da detecção de planos. As interações de arrastar e rotacionar foram desativadas para manter o personagem associado ao marcador. A escolha também reduz a quantidade de comportamentos que precisam ser validados no escopo do teste.

A lógica usa os componentes do AR Foundation, mas o projeto foi direcionado ao Android. Isso oferece uma base para avaliar outros provedores no futuro; não significa que outras plataformas já tenham sido configuradas ou testadas.

## Organização do projeto

| Arquivo ou diretório | Responsabilidade |
| --- | --- |
| [AR_Scene.unity](Assets/Scenes/AR_Scene.unity) | Cena principal, habilitada na configuração de build. |
| [TrackedImagePlacement.cs](Assets/Scripts/TrackedImagePlacement.cs) | Localiza o marcador rastreado, calcula o posicionamento e controla a criação, substituição e visibilidade do personagem. |
| [MarkerCharacterMenu.cs](Assets/Scripts/MarkerCharacterMenu.cs) | Controla o menu e a disponibilidade do botão de seleção. |
| [CharacterTapReaction.cs](Assets/Scripts/CharacterTapReaction.cs) | Identifica o toque, aciona a reação e controla o áudio associado. |
| `Assets/Prefabs/` | Prefabs dos personagens e Animator Controllers. |
| `Assets/Markers/` | Imagem-alvo, biblioteca de referências e material do indicador. |
| `Assets/Simulation/` | Ambiente utilizado na simulação de AR no Editor. |

O menu delega a criação ao componente de posicionamento. Cada personagem controla sua própria reação ao toque. Essa divisão permite ajustar a interface, o rastreamento ou as animações sem reunir todas as responsabilidades em um único script.

## Como abrir e executar no Unity

1. Instale o **Unity 6000.6.9f1** pelo Unity Hub. Para gerar o APK, inclua os módulos **Android Build Support**, **Android SDK & NDK Tools** e **OpenJDK**.
2. Clone ou baixe este repositório e adicione a pasta do projeto ao Unity Hub.
3. Abra o projeto com a versão indicada e aguarde a importação dos assets e a resolução dos pacotes.
4. Abra `Assets/Scenes/AR_Scene.unity`.
5. Para testar no Editor, selecione `Assets/Simulation/MarkerSimulationEnvironment.prefab` como ambiente de XR Simulation, nas configurações de simulação do XR Plug-in Management. Essa seleção pode precisar ser refeita em outra máquina, pois é uma preferência do Editor.
6. Entre em Play Mode. Com o marcador simulado rastreado, use **Choose character**, escolha um personagem e clique e solte sobre ele para acionar a reação.
7. Navegue pelo ambiente com o botão direito do mouse e as teclas WASD.

Não adicione uma segunda cópia do ambiente de simulação à cena. A simulação utiliza um marcador configurado no ambiente; ela verifica o fluxo de posicionamento e interação, mas não reproduz o reconhecimento dos pixels por uma câmera real.

## Como gerar o APK

1. Abra o projeto e aguarde a conclusão da importação.
2. Em **File > Build Profiles**, selecione ou crie um perfil Android e ative-o.
3. Confirme que `Assets/Scenes/AR_Scene.unity` está habilitada na lista de cenas do build. Essa já é a cena registrada na configuração do projeto.
4. Em **Project Settings > XR Plug-in Management**, confirme que o provedor ARCore está habilitado para Android.
5. Confira as configurações Android: API mínima 26, arquitetura ARM64 e ferramentas Android instaladas pelo Unity Hub. Preserve as demais configurações do projeto para reproduzir o build existente.
6. Para gerar um APK instalável diretamente, mantenha a geração de Android App Bundle desativada.
7. Execute **Build** e escolha o destino do APK. Para instalar pelo Editor, conecte um aparelho com depuração USB habilitada e utilize **Build And Run**.
8. Execute o roteiro de teste em dispositivo físico antes de disponibilizar uma nova release.

## Processo de desenvolvimento e validação

### Protótipo inicial

O template AR Mobile foi utilizado como base. Em uma cópia da cena padrão, um modelo GLB provisório foi importado com glTFast e convertido em prefab. Suas animações foram organizadas em um Animator Controller, permitindo validar a reação acionada por um Trigger.

Após os testes iniciais, os objetos de exemplo e as interações de manipulação foram removidos ou desativados para adequar o comportamento ao posicionamento pelo marcador.

### Integração do marcador e dos personagens

Um QR code foi gerado com Python e incorporado à imagem de referência. A leitura do código foi testada separadamente; esse teste não corresponde à implementação de decodificação dentro do aplicativo.

A imagem foi cadastrada na biblioteca de referências e representada no ambiente de simulação. O posicionamento em superfícies foi substituído pela criação em um ponto da imagem reconhecida. Depois da validação do posicionamento e do toque, foram preparados os quatro prefabs finais e a troca de personagem pelo menu.

### Ajustes após testes em Android

Os testes mostraram que o fluxo funcionava na simulação, mas a experiência no dispositivo físico era inconsistente quando a câmera se afastava do marcador. Isso dificultava observar os modelos e suas animações.

Os modelos foram reduzidos e a dependência da detecção de planos foi removida. A lógica também passou a manter um personagem existente na pose retida do marcador durante o rastreamento limitado da imagem, desde que a sessão continuasse rastreando o ambiente.

Os testes posteriores confirmaram uma interação mais fluida e maior estabilidade na posição dos personagens após os ajustes.

### Estado da validação

- **Simulação no Editor:** utilizada durante o desenvolvimento para verificar posicionamento, seleção e reação ao toque.
- **Dispositivo físico:** funcionamento validado em Android, com interação mais fluida e maior estabilidade na posição dos personagens após os ajustes. Modelo do aparelho, versão do Android e identificação exata do APK testado ainda não foram documentados.
- **Build:** os registros locais consultados indicam builds concluídos com zero erros e 358 avisos. Esses registros não substituem a validação funcional de cada release.
- **Animação contínua:** reprodução em loop do estado `idle` validada nos quatro personagens durante os testes.
- **Pendências:** completar o indicador explícito de estado e documentar o teste da versão distribuída.

### Roteiro para validar uma release

1. Registrar aparelho, versão do Android e identificação da release.
2. Verificar o comportamento antes do reconhecimento e a resposta ao detectar o marcador.
3. Selecionar cada um dos quatro personagens e verificar sua animação em repouso.
4. Tocar em cada personagem, confirmar animação e som e verificar o retorno ao repouso.
5. Trocar de personagem e confirmar que apenas um permanece na cena.
6. Mover o dispositivo sem mover o marcador e observar a estabilidade da posição.
7. Afastar a câmera, perder e recuperar o rastreamento e conferir o comportamento descrito abaixo.

Esse roteiro descreve as verificações esperadas; não constitui um registro de aprovação de todos os itens.

## Limitações conhecidas

- A criação inicial exige rastreamento completo da imagem. A interface ainda precisa apresentar explicitamente o estado de busca.
- A posição é derivada do rastreamento da imagem. Não foi implementada persistência de âncoras entre sessões, nem há garantia de posição permanente diante de qualquer perda de rastreamento.
- Quando o rastreamento da imagem fica limitado, um personagem já criado permanece visível e pode ser substituído enquanto a sessão de AR continua rastreando. Esse comportamento pressupõe que o marcador permaneça parado enquanto não está sendo visto com clareza.
- Se o rastreamento da imagem passa a `None`, ou se a sessão perde o rastreamento durante o estado limitado, o personagem é ocultado. A mesma instância pode reaparecer quando o marcador é reconhecido novamente.
- A biblioteca considera a imagem completa com 7 cm de largura. Para utilizar outra dimensão física, atualize a largura cadastrada em `Assets/Markers/MarkerReferenceLibrary.asset` e valide novamente o posicionamento e a escala.
- Não foram implementadas interação entre múltiplos personagens nem persistência da escolha entre sessões.
- Não há validação documentada em outras plataformas ou em uma variedade de aparelhos Android.
- Não foram registradas medições de FPS, uso de memória ou consumo de bateria. O projeto não apresenta otimização de performance como diferencial validado.

## O que faria com mais tempo

1. Concluir o indicador explícito de estados, priorizando o núcleo obrigatório.
2. Documentar os testes por dispositivo e release, incluindo perda de rastreamento, iluminação e distância de reconhecimento.
3. Melhorar a orientação inicial da interface e padronizar seus textos em português.
4. Medir desempenho e tamanho do APK, revisar dependências e investigar os avisos de build antes de propor otimizações.
5. Avaliar persistência da última escolha e suporte a outra plataforma, mantendo separados o posicionamento, a interface e as reações.

## Uso de inteligência artificial

Ferramentas de inteligência artificial auxiliaram na elaboração de scripts e em alterações de configuração do Unity. O processo de desenvolvimento e os ajustes resultantes dos testes estão descritos acima para tornar as decisões e limitações explícitas.
