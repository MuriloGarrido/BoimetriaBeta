<div align="center">

# 🐂 Boimetria

**Identificação biométrica de bovinos pelo focinho — 100% no dispositivo, sem internet.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-Android-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/maui/)
[![ONNX Runtime](https://img.shields.io/badge/ONNX%20Runtime-on--device-005CED?logo=onnx&logoColor=white)](https://onnxruntime.ai/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

</div>

---

## ⚠️ Status: Prova de Conceito (PoC) Beta

Este projeto é uma **prova de conceito** para validar a integração de:
- **YOLO** (detecção de objetos) em Android
- **MAUI** (.NET cross-platform)
- **ONNX Runtime** (inferência on-device)

**Não é um produto pronto para produção.** Use para fins educacionais, pesquisa e experimentação.

---

## 📖 Sobre o projeto

O focinho de um bovino é tão único quanto a impressão digital de uma pessoa: o padrão de sulcos e cristas
não se repete entre animais. **Boimetria** explora essa característica como base para um identificador biométrico
— basta apontar a câmera do celular para o focinho do boi.

A aplicação roda **inteiramente no dispositivo Android**, usando um modelo YOLO exportado para ONNX e inferência
local com **ONNX Runtime**. Não há servidor, não há nuvem e **não é necessária conexão com a internet**.

### Por que este PoC é interessante

- **IA embarcada (edge AI):** validação prática de YOLO + ONNX Runtime rodando localmente em Android, sem latência de rede.
- **MAUI em produção:** demonstração real de um único código C# compilado para Android via .NET MAUI, com acesso aos APIs nativos.
- **Prototipagem rápida:** stack moderno e estruturado (MVVM + DI) para experimentos com visão computacional em mobile.
- **Offline-first:** requisito crítico para campo onde não há cobertura de rede.

---

## ✨ Funcionalidades

| Recurso | Descrição |
|---|---|
| 📷 **Captura e identificação** | Tira uma foto (ou escolhe da galeria) e o app localiza o focinho do animal automaticamente. |
| 🎯 **Detecção com IA on-device** | Modelo ONNX processado localmente com pré-processamento de imagem 640×640 e filtro por confiança. |
| 🖼️ **Resultado visual** | A imagem original recebe um *bounding box* e um selo de confiança; o focinho é recortado em destaque. |
| ⚙️ **Preparação do sistema** | O modelo (`.onnx`) é carregado pelo usuário em *Ajustes* e persistido localmente para uso offline. |
| 📊 **Relatórios e manejo** | Estrutura de navegação pronta para os próximos módulos (vacinação, pesagem, etc.). |

---

## 🏗️ Arquitetura

O projeto segue o padrão **MVVM** (Model–View–ViewModel) com **injeção de dependência**, separando
claramente UI, lógica de apresentação e serviços de domínio.

```
┌─────────────────────────────────────────────────────────────┐
│                          Views (XAML)                        │
│      MainPage · IdentificationPage · ReportsPage · Settings  │
└───────────────────────────────┬─────────────────────────────┘
                                 │  data binding (CommunityToolkit.Mvvm)
┌───────────────────────────────▼─────────────────────────────┐
│                         ViewModels                           │
│   Main · Identification · Reports · Settings (BaseViewModel) │
└───────────────────────────────┬─────────────────────────────┘
                                 │  interfaces + DI
┌───────────────────────────────▼─────────────────────────────┐
│                          Services                            │
│  ModelService (ONNX)  ·  ImageProcessingService (SkiaSharp)  │
│  ImagePickerService   ·  SettingsService  ·  DialogService   │
└───────────────────────────────┬─────────────────────────────┘
                                 │
┌───────────────────────────────▼─────────────────────────────┐
│                           Models                             │
│              DetectionResult  ·  BoundingBox                 │
└──────────────────────────────────────────────────────────────┘
```

### Como funciona a identificação (pipeline)

1. **Entrada** — o usuário captura ou seleciona uma foto (`ImagePickerService`).
2. **Pré-processamento** — a imagem é decodificada com **SkiaSharp**, redimensionada para `640×640` e
   convertida em um tensor `[1, 3, 640, 640]` com canais RGB normalizados (`ModelService`).
3. **Inferência** — o tensor passa pela **ONNX Runtime** (`InferenceSession`), executada em uma *thread*
   de background com otimização de grafo e *threading* ajustado ao número de núcleos.
4. **Pós-processamento** — a melhor detecção acima do limiar de confiança (`0.5`) é selecionada; as
   coordenadas são reescaladas para a resolução original da foto.
5. **Saída visual** — `ImageProcessingService` desenha o *bounding box* arredondado + selo de confiança e
   recorta a região do focinho, exibidos lado a lado na tela de resultado.

---

## 🧰 Stack técnica

- **Linguagem:** C# (nullable + implicit usings habilitados)
- **Framework:** [.NET 10](https://dotnet.microsoft.com/) / [.NET MAUI](https://learn.microsoft.com/dotnet/maui/)
- **Padrão de UI:** MVVM com [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) (source generators)
- **Componentes:** [CommunityToolkit.Maui](https://learn.microsoft.com/dotnet/communitytoolkit/maui/)
- **Inferência de IA:** [Microsoft.ML.OnnxRuntime](https://onnxruntime.ai/)
- **Processamento de imagem:** [SkiaSharp](https://github.com/mono/SkiaSharp)
- **Plataforma-alvo:** Android (API 29+)

## ⚡ Limitações (Beta/PoC)

- O modelo YOLO é uma prova de conceito e pode ter variações de precisão em diferentes condições de luz e ângulos
- Não há banco de dados persistente de animais (módulos de manejo são estrutura UI apenas)
- Performance depende do dispositivo Android (número de núcleos, RAM disponível)
- Não há versionamento ou atualização automática do modelo
- Interface é em português (português brasileiro)

---

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Workload do MAUI instalado:
  ```bash
  dotnet workload install maui
  ```
- **Android SDK** / emulador configurado (via Visual Studio 2022+ ou Android Studio)
- Um dispositivo Android conectado ou emulador rodando (Android API 29 ou superior)

### Build e execução

```bash
# Clonar
git clone https://github.com/MuriloGarrido/BoimetriaBeta.git
cd BoimetriaBeta

# Restaurar dependências
dotnet restore

# Rodar no Android (emulador/dispositivo conectado)
dotnet build BoimetriaBeta/BoimetriaBeta.csproj -t:Run -f net10.0-android
```

Também é possível abrir `BoimetriaBeta.slnx` diretamente no Visual Studio 2022+ e:
1. Selecionar **Android Emulator** ou **Android Device** na barra de ferramentas
2. Pressionar **F5** (Debug) ou **Ctrl+F5** (Release)

### Preparando o modelo

O peso da rede neural (`.onnx`) **não é versionado** no repositório (ver `.gitignore`). Para usar o app:

1. Abra o app no dispositivo/emulador e navegue até **Ajustes**.
2. Toque em **Carregar arquivo** e selecione o modelo `.onnx` da galeria do dispositivo.
3. O arquivo é copiado para o armazenamento privado do app e fica disponível offline.

**Requisitos do modelo:**
- Entrada: tensor `640×640` (imagem RGB)
- Saída: formato YOLO-padrão `[1, N, ≥6]` onde cada detecção contém:
  - `x1, y1, x2, y2` — coordenadas do bounding box
  - `confiança` — score de detecção
  - `classe` — índice da classe (0 = focinho de bovino esperado)

---

## 📂 Estrutura do projeto

```
BoimetriaBeta/
├── BoimetriaBeta.slnx              # Solução
├── BoimetriaBeta/
│   ├── Models/                     # DetectionResult, BoundingBox
│   ├── Services/                   # ONNX, imagem, picker, settings, diálogos (+ interfaces)
│   ├── ViewModels/                 # Lógica de apresentação (MVVM)
│   ├── Views/                      # Telas XAML
│   ├── Platforms/
│   │   └── Android/                # Código específico do Android
│   ├── Resources/                  # Ícones, fontes, estilos, splash
│   ├── AppShell.xaml               # Navegação (TabBar)
│   ├── MauiProgram.cs              # Bootstrap + injeção de dependência
│   └── BoimetriaBeta.csproj        # Configuração do projeto
├── .gitignore
├── LICENSE
└── README.md
```

### Pastas principais

- **Models/** — Classes de domínio (`DetectionResult`, `BoundingBox`)
- **Services/** — Interfaces e implementações:
  - `IModelService` — carregamento e inferência ONNX
  - `IImageProcessingService` — redimensionamento, normalização, desenho de caixas
  - `IImagePickerService` — captura e seleção de fotos
  - `ISettingsService` — persistência de configurações
  - `IDialogService` — exibição de alertas/diálogos
- **ViewModels/** — Lógica de apresentação com binding bidirecional
- **Views/** — Páginas XAML (`MainPage`, `IdentificationPage`, `ReportsPage`, `SettingsPage`)
- **Platforms/Android/** — Permissões, integrações Android específicas

---

## 🔧 Desenvolvimento

### Permissões Android

O app requer as seguintes permissões no `AndroidManifest.xml`:

```xml
<uses-permission android:name="android.permission.CAMERA" />
<uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
<uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
```

Em Android 6.0+, o app solicita permissões em tempo de execução via `MAUI Permissions API`.

### Inferência ONNX em background

A detecção roda em uma thread de background para não congelar a UI:

```csharp
var result = await Task.Run(() => modelService.Detect(image));
```

### Otimizações para Android

- **Grafo ONNX:** otimizado com `SessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL`
- **Threading:** ajustado ao número de núcleos (`Environment.ProcessorCount`)
- **Memória:** imagens e tensores são descartados após uso (`Dispose()`)

---

## 🗺️ Roadmap (Planejado)

> ⚠️ Itens abaixo são apenas planejamento. A versão atual é PoC focado em YOLO + MAUI + ONNX Runtime.

- [x] Detecção YOLO com ONNX Runtime em Android
- [x] Tela de preparação do sistema (carregar modelo)
- [ ] Cadastro e *matching* de animais (associar focinho a um indivíduo)
- [ ] Módulos de manejo (vacinação, pesagem, histórico)
- [ ] Relatórios por período
- [ ] Sincronização opcional com a nuvem

---

## 🤝 Contribuindo

Sugestões e PRs são bem-vindas! Antes de submeter:

1. Mantenha o código alinhado com o padrão MVVM e DI existente
2. Teste em um dispositivo Android real ou emulador
3. Documente mudanças significativas na UI ou serviços

---

## 📄 Licença

Distribuído sob a licença MIT. Veja [LICENSE](LICENSE) para mais detalhes.

---

<div align="center">
Desenvolvido por <a href="https://github.com/MuriloGarrido">Murilo Garrido</a>
</div>
