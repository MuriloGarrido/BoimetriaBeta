<div align="center">

# 🐂 Boimetria

**Identificação biométrica de bovinos pelo focinho — 100% no dispositivo, sem internet.**

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-Android%20%7C%20iOS%20%7C%20Windows%20%7C%20macOS-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/dotnet/maui/)
[![ONNX Runtime](https://img.shields.io/badge/ONNX%20Runtime-on--device-005CED?logo=onnx&logoColor=white)](https://onnxruntime.ai/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

</div>

---

## 📖 Sobre o projeto

O focinho de um bovino é tão único quanto a impressão digital de uma pessoa: o padrão de sulcos e cristas
não se repete entre animais. **Boimetria** transforma essa característica em um identificador biométrico
prático para o produtor rural — basta apontar a câmera do celular para o focinho do boi.

A aplicação roda **inteiramente no dispositivo**, usando um modelo de visão computacional (detecção de
objetos no estilo YOLO, exportado para ONNX). Não há servidor, não há nuvem e **não é necessária conexão
com a internet** — um requisito-chave para uso em campo, onde a cobertura de rede costuma ser ruim ou
inexistente.

> **Status:** beta funcional. O reconhecimento do focinho está operacional; módulos de manejo e relatórios
> estão sinalizados na interface como evolução planejada (ver [Roadmap](#-roadmap)).

### Por que este projeto é interessante

- **IA embarcada (edge AI):** inferência de rede neural local, sem latência de rede e com privacidade total dos dados.
- **App multiplataforma real:** um único código-base C# rodando em Android, iOS, Windows e macOS via .NET MAUI.
- **Pensado para o usuário final:** linguagem da interface no tom do produtor rural, fluxo de preparação simples e
  tratamento de erros amigável ("Não achei o boi, tenta de mais perto com boa luz").

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
- **Plataformas-alvo:** Android · iOS · Windows · macOS (Mac Catalyst)

---

## 🚀 Como rodar

### Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Workload do MAUI instalado:
  ```bash
  dotnet workload install maui
  ```
- Para Android: Android SDK / emulador (via Visual Studio 2022+ ou Android Studio).
  Para iOS/macOS: um host macOS com Xcode.

### Build e execução

```bash
# Clonar
git clone https://github.com/MuriloGarrido/BoimetriaBeta.git
cd BoimetriaBeta

# Restaurar dependências
dotnet restore

# Rodar no Android (emulador/dispositivo conectado)
dotnet build BoimetriaBeta/BoimetriaBeta.csproj -t:Run -f net10.0-android

# Ou no Windows
dotnet build BoimetriaBeta/BoimetriaBeta.csproj -t:Run -f net10.0-windows10.0.19041.0
```

> Também é possível abrir `BoimetriaBeta.slnx` diretamente no Visual Studio 2022+ e selecionar o alvo desejado.

### Preparando o modelo

O peso da rede neural (`.onnx`) **não é versionado** no repositório (ver `.gitignore`). Para usar o app:

1. Abra o app e vá em **Ajustes**.
2. Toque em **Carregar arquivo** e selecione o modelo `.onnx`.
3. O arquivo é copiado para o armazenamento local do app e fica disponível offline.

O modelo esperado é um detector de objetos com entrada `640×640` e saída no formato
`[1, N, ≥6]` (`x1, y1, x2, y2, confiança, classe`).

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
│   ├── Platforms/                  # Código específico de Android/iOS/Windows/macOS
│   ├── Resources/                  # Ícones, fontes, estilos, splash
│   ├── AppShell.xaml               # Navegação (TabBar)
│   └── MauiProgram.cs              # Bootstrap + injeção de dependência
├── .gitignore
├── LICENSE
└── README.md
```

---

## 🗺️ Roadmap

- [x] Identificação do focinho por IA on-device
- [x] Tela de preparação do sistema (carregar modelo)
- [ ] Cadastro e *matching* de animais (associar focinho a um indivíduo)
- [ ] Módulos de manejo (vacinação, pesagem, histórico)
- [ ] Relatórios por período
- [ ] Sincronização opcional com a nuvem

---

## 📄 Licença

Distribuído sob a licença MIT. Veja [LICENSE](LICENSE) para mais detalhes.

---

<div align="center">
Desenvolvido por <a href="https://github.com/MuriloGarrido">Murilo Garrido</a>
</div>
