# Agenda de Licitações - WinUI 3

teste

Aplicação de gerenciamento de licitações públicas migrada para **WinUI 3** com tema **Mica**.

## ✨ Características Principais

- **Interface Moderna**: Tema Fluent Design com efeito Mica backdrop
- **Calendário Interativo**: Visualização mês, semana e ano
- **Filtros Avançados**: Busca por estado, município, ano, status, período e itens
- **Gerenciamento de Licitações**: CRUD completo com histórico
- **Abas de Detalhe**: Diário, Itens, Anexos e Histórico
- **Armazenamento Local**: JSON para persistência de dados

## 🎯 Requisitos

- **.NET 8.0** ou superior
- **Windows 10 (build 1809+)** ou Windows 11
- **Visual Studio 2022** (com suporte a WinUI 3)

## 📁 Estrutura de Arquivos

### Arquivos Principais

| Arquivo | Descrição |
|---------|-----------|
| `App.xaml` | Recursos de aplicação e tema Mica |
| `App.xaml.cs` | Entry point da aplicação |
| `MainWindow.xaml` | Janela principal com sidebar e toolbar |
| `MainWindow.xaml.cs` | Lógica da janela principal |
| `CalendarPanel.cs` | Controle customizado para calendários |
| `FormLicitacaoDialog.xaml` | Dialog para criar/editar licitações |
| `FormLicitacaoDialog.xaml.cs` | Lógica do formulário |
| `DetalhesDialog.xaml` | Detalhes com TabView (4 abas) |
| `DetalhesDialog.xaml.cs` | Lógica dos detalhes |
| `Dialogs.cs` | Dialogs auxiliares e helpers |
| `DataService.cs` | Serviço de persistência (JSON) |
| `Models.cs` | Modelos de domínio |
| `Program.cs` | Entry point console |
| `app.manifest` | Compatibilidade Windows |
| `AgendaLicitacoes.csproj` | Configuração do projeto |

## 🚀 Como Executar

1. Abra o projeto no Visual Studio 2022
2. Restaure as dependências NuGet:
   ```
   dotnet restore
   ```
3. Execute o projeto:
   ```
   dotnet run
   ```

Ou, diretamente do Visual Studio:
- Pressione `F5` para executar em debug
- Pressione `Ctrl+F5` para executar sem debug

## 🎨 Tema Visual

A aplicação usa:
- **Cores Neutras**: Cinza claro (#F5F5F5), branco (#FFFFFF)
- **Destaque**: Azul (#1A73E8)
- **Status Colors**:
  - 🟢 Ganho: #34A853
  - 🔴 Perdido: #D33B27
  - 🟠 Suspenso: #EA8600
  - 🔵 ATA: #4285F4

## 📊 Funcionalidades

### Dashboard Principal
- Sidebar com filtros (Estado, Município, Ano, Status, Data, Item)
- Toolbar com navegação e seleção de visualização
- Calendário interativo (mês/semana/ano)
- Vista em lista com DataGrid
- Estatísticas do mês

### Licitação
- Campos: Ano, Estado, Tipo, Número, Município, Portal, Órgão
- Dados Adicionais: Código BB, Data/Hora, Status, Valor, Código Effecti, UASG
- Validação automática de campos obrigatórios

### Detalhes
- **Diário**: Editor de texto para anotações
- **Itens**: DataGrid com número, código, descrição, quantidade, valor
- **Anexos**: Gerenciar arquivos ligados à licitação
- **Histórico**: Log de alterações com timestamp

## 🔄 Fluxo de Dados

```
App.xaml.cs (Initializer)
    ↓
MainWindow.xaml (Shell)
    ├─ CalendarPanel (Visualização)
    ├─ DataGrid (Lista)
    └─ Dialogs (Criar/Editar/Detalhar)
        ↓
    DataService (Persistência JSON)
        ↓
    licitacoes_data.json
    licitacoes_config.json
```

## 🛠️ Desenvolvimento

### Adicionar Novo Filtro
1. Adicione campo em `FiltroState` (Models.cs)
2. Adicione controle no XAML (MainWindow.xaml)
3. Implemente handler em MainWindow.xaml.cs
4. Atualize `DataService.Filtrar()`

### Adicionar Nova Aba
1. Adicione `TabViewItem` em DetalhesDialog.xaml
2. Implemente lógica em DetalhesDialog.xaml.cs
3. Atualize Models.cs se necessário

## 📝 Notas de Migração

- ✅ DataService mantido 100% intacto
- ✅ Models adaptados para remover System.Drawing
- ✅ WinForms → WinUI 3 controles equivalentes
- ✅ Diálogos modais → ContentDialog/Window
- ✅ DataGridView → DataGrid (XAML)
- ⚠️ Custom painting → XAML native rendering

## 🐛 Troubleshooting

### Erro: "Windows App SDK not found"
```bash
dotnet add package Microsoft.WindowsAppSDK
```

### Erro: "Platform not supported"
Certifique-se que está usando Windows 10 (build 1809+) ou Windows 11.

### Erro de permissão ao criar pastas
Verifique as permissões da pasta raiz detectada automaticamente.

## 📦 Dependências

- `Microsoft.WindowsAppSDK` (1.6+)
- `Newtonsoft.Json` (13.0.3)

## 📄 Licença

DIMAVE - 2024

---

**Versão**: 1.0.0 WinUI 3  
**Atualizado**: Abril 2024
