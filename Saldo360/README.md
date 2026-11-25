# Saldo360 — Assistente Financeiro Pessoal

App de controle financeiro em C# / .NET MAUI (nível básico-intermediário).

## Como abrir
1. Abra `Saldo360.slnx` no Visual Studio (workload **.NET MAUI** instalada).
2. Aguarde o restore dos pacotes e escolha um emulador Android ou *Windows Machine*.
3. Execute (F5).

## Navegação (4 tipos)
- **Menu lateral (Flyout):** botão ☰ no topo das telas ou deslizar da borda esquerda; lista as 4 telas principais e **Sobre** (`AppShell.xaml`)
- **Abas (Tab):** menu inferior com as 4 telas principais
- **Pilha (Stack):** Movimentações → Detalhes, com botão voltar
- **Modal:** Nova movimentação abre por cima das outras telas

## Telas
| Tela | O que faz |
|---|---|
| **Início** (Tab) | Saldo do mês, receitas, despesas, gastos por categoria e cotação do dólar/euro |
| **Movimentações** (Tab) | Lista de lançamentos; tocar em um item abre os **Detalhes** (navegação **Stack**) |
| **Nova movimentação** (**Modal**) | Formulário para adicionar receita ou despesa |
| **Planejamento** (Tab) | Limites mensais por categoria (com barra de progresso) e uma meta de economia |
| **Economia** (Tab) | Câmbio (AwesomeAPI) e inflação IPCA (IBGE) |
| **Simulador** (só no menu lateral) | Cruza as duas APIs: corrige um valor pela inflação (IBGE), compara com o dólar no mesmo período (AwesomeAPI) e converte para dólar e euro |
| **Dólar x inflação** (só no menu lateral) | Compara, mês a mês, o IPCA (IBGE) com a variação do dólar (AwesomeAPI) |
| **Sobre** (só no menu lateral) | Resumo da navegação, fontes de dados e botão para apagar todos os dados |

## APIs usadas (as duas sem chave)
- **AwesomeAPI** — `https://economia.awesomeapi.com.br/json/last/USD-BRL,EUR-BRL`
- **IBGE / SIDRA** — `https://servicodados.ibge.gov.br/api/v3/agregados/1737/periodos/-6/variaveis/63?localidades=N1[all]`
  (IPCA, variação mensal, últimos 6 meses)

## Como as duas APIs conversam (Simulador)
`Services/SimulationService.cs` encadeia as chamadas:
1. **IBGE** devolve o IPCA dos últimos 6 meses → o app obtém a inflação acumulada **e o período** (ex.: 01/05 a 31/10).
2. Esse período é enviado para a **AwesomeAPI** (`/json/daily/USD-BRL/360?start_date=...&end_date=...`), que devolve o dólar no início e no fim.
3. O app compara a variação do dólar com a inflação ("dólar real") e converte o valor corrigido para dólar e euro pela cotação de hoje.

### Comparativo mensal (Dólar x inflação)
`Services/ComparisonService.cs` faz uma segunda troca de dados entre as APIs:
1. **IBGE** devolve os meses e o IPCA de cada um.
2. A lista de meses vira o intervalo enviado à **AwesomeAPI**, que devolve o dólar dia a dia; o app pega o fechamento de cada mês.
3. O app calcula a variação do dólar mês a mês e mostra ao lado do IPCA, com um resumo ("o dólar subiu mais que a inflação em X de Y meses") e o acumulado.

Se o histórico do dólar falhar, o simulador continua funcionando só com a inflação e a conversão.

## Sem banco de dados
Não há mais SQLite. Os dados ficam em listas na memória (`Services/FinanceStore.cs`) e são salvos
como JSON em `Preferences` do MAUI, então continuam lá quando o app é fechado e aberto de novo.

## Estrutura
- `Models/` — classes de dados (Movement, Budget, Goal, categorias)
- `Services/` — `FinanceStore` (dados e cálculos), `CurrencyApiService` e `IbgeApiService` (APIs)
- `Views/` — as telas (XAML + code-behind)
- `Controls/` — `MenuButton`, o botão ☰ que abre o menu lateral
- `Helpers/` — formatação de R$, cores e layout responsivo
- `App.xaml` — cores e estilos de todo o app
