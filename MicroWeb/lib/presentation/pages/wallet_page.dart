import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../logic/wallets/wallets_cubit.dart';
import '../../data/models/currency.dart';
import '../../data/models/ledger_type.dart';

class WalletPage extends StatelessWidget {
  const WalletPage({super.key});

  Future<void> _showTopUpDialog(BuildContext context) async {
    final amounts = <int>[10, 100, 1000];
    int selectedAmount = amounts.first;
    Currency selectedCurrency = Currency.EUR;
    final descCtrl = TextEditingController();

    await showDialog(
      context: context,
      builder: (ctx) {
        return AlertDialog(
          title: const Text('Add money'),
          content: StatefulBuilder(
            builder: (context, setState) {
              return SizedBox(
                width: 360,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Row(
                      children: [
                        const Text('Amount'),
                        const SizedBox(width: 16),
                        Expanded(
                          child: DropdownButton<int>(
                            value: selectedAmount,
                            isExpanded: true,
                            items: amounts
                                .map((a) => DropdownMenuItem<int>(value: a, child: Text(a.toString())))
                                .toList(),
                            onChanged: (v) => setState(() => selectedAmount = v ?? selectedAmount),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    Row(
                      children: [
                        const Text('Currency'),
                        const SizedBox(width: 16),
                        Expanded(
                          child: DropdownButton<Currency>(
                            value: selectedCurrency,
                            isExpanded: true,
                            items: Currency.values
                                .map((c) => DropdownMenuItem<Currency>(value: c, child: Text(c.name)))
                                .toList(),
                            onChanged: (v) => setState(() => selectedCurrency = v ?? selectedCurrency),
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    TextField(
                      controller: descCtrl,
                      decoration: const InputDecoration(labelText: 'Description (optional)'),
                    ),
                  ],
                ),
              );
            },
          ),
          actions: [
            TextButton(onPressed: () => Navigator.of(ctx).pop(), child: const Text('Cancel')),
            FilledButton(
              onPressed: () async {
                await context.read<WalletCubit>().topUp(
                  amountMajor: selectedAmount,
                  currency: selectedCurrency,
                  description: descCtrl.text.isEmpty ? null : descCtrl.text,
                );
                if (ctx.mounted) Navigator.of(ctx).pop();
                if (context.mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Top-up request sent')));
                }
              },
              child: const Text('Top up'),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: BlocBuilder<WalletCubit, WalletState>(
        builder: (context, state) {
          if (state.loading) {
            return const Center(child: CircularProgressIndicator());
          }
          if (state.error != null) {
            return Center(child: Text('Failed to load wallet: ${state.error}'));
          }
          return Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  const Icon(Icons.account_balance_wallet, size: 28),
                  const SizedBox(width: 8),
                  Text('Wallet', style: Theme.of(context).textTheme.titleLarge),
                  const Spacer(),
                  if (state.currentUser != null) ...[
                    Text('User: ${state.currentUser!.email ?? state.currentUser!.displayName ?? ''}'),
                    const SizedBox(width: 12),
                    ElevatedButton.icon(
                      onPressed: () => _showTopUpDialog(context),
                      icon: const Icon(Icons.add_circle_outline),
                      label: const Text('Add money'),
                    ),
                  ],
                ],
              ),
              const SizedBox(height: 12),
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(12.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('Balances', style: TextStyle(fontWeight: FontWeight.bold)),
                      const SizedBox(height: 8),
                      if (state.wallet?.balances == null || state.wallet!.balances!.isEmpty)
                        const Text('No balances')
                      else
                        Wrap(
                          spacing: 12,
                          runSpacing: 8,
                          children: [
                            for (final b in state.wallet!.balances!)
                              Chip(label: Text('${b.currency?.name ?? ''}: ${(b.balanceMinor ?? 0) / 100}')),
                          ],
                        ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              Expanded(
                child: Card(
                  clipBehavior: Clip.antiAlias,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Container(
                        color: Theme.of(context).colorScheme.surfaceContainerHighest,
                        padding: const EdgeInsets.all(12),
                        child: const Text('History'),
                      ),
                      Expanded(
                        child: state.ledger.isEmpty
                            ? const Center(child: Text('No history'))
                            : ListView.separated(
                                itemCount: state.ledger.length,
                                separatorBuilder: (_, __) => const Divider(height: 1),
                                itemBuilder: (ctx, i) {
                                  final it = state.ledger[i];
                                  final subtitleParts = [
                                    if (it.type != null) 'Type: ${it.type!.name}',
                                    if (it.createdAt != null) 'Date: ${it.createdAt}',
                                    if (it.description != null) it.description!,
                                  ];
                                  final bg = it.type == LedgerType.credit
                                      ? Colors.green.shade50
                                      : (it.type == LedgerType.debit ? Colors.red.shade50 : null);
                                  return ListTile(
                                    tileColor: bg,
                                    leading: const Icon(Icons.receipt_long),
                                    title: Text('${(it.amountMinor ?? 0) / 100} ${it.currency?.name ?? ''}'),
                                    subtitle: Text(subtitleParts.join('  •  ')),
                                  );
                                },
                              ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}
