import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../data/models/currency.dart';
import '../../data/models/requests/create_account_request.dart';
import '../../data/models/responses/account_response.dart';
import '../../logic/accounts/accounts_bloc.dart';

class AccountsListPage extends StatelessWidget {
  const AccountsListPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              const Expanded(
                child: Text('Accounts', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              ),
              FilledButton.icon(
                onPressed: () async {
                  await showDialog(
                    context: context,
                    builder: (_) =>
                        BlocProvider.value(value: context.read<AccountsBloc>(), child: const _NewAccountDialog()),
                  );
                },
                icon: const Icon(Icons.add),
                label: const Text('New'),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: BlocBuilder<AccountsBloc, AccountsState>(
              builder: (context, state) {
                if (state.loading) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (state.error != null) {
                  return Center(child: Text('Failed to load: ${state.error}'));
                }
                if (state.items.isEmpty) {
                  return const Center(child: Text('No accounts yet.'));
                }
                return LayoutBuilder(
                  builder: (ctx, constraints) {
                    final crossAxis = constraints.maxWidth ~/ 320;
                    return GridView.builder(
                      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: crossAxis.clamp(1, 4),
                        childAspectRatio: 3,
                        crossAxisSpacing: 12,
                        mainAxisSpacing: 12,
                      ),
                      itemCount: state.items.length,
                      itemBuilder: (ctx, i) {
                        final item = state.items[i];
                        return _AccountCard(item: item);
                      },
                    );
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}

class _AccountCard extends StatelessWidget {
  const _AccountCard({required this.item});
  final AccountResponse item;

  @override
  Widget build(BuildContext context) {
    final iban = item.iban ?? '(no IBAN)';
    final cur = item.currency?.name ?? '';
    return Card(
      child: InkWell(
        onTap: () async {
          final refresh = await context.push('/accounts/${item.id}', extra: item);
          if (refresh == true && context.mounted) {
            context.read<AccountsBloc>().load();
          }
        },
        child: Padding(
          padding: const EdgeInsets.all(12.0),
          child: Row(
            children: [
              const Icon(Icons.account_balance, size: 36),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(iban, style: const TextStyle(fontWeight: FontWeight.bold)),
                    Text(cur),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _NewAccountDialog extends StatefulWidget {
  const _NewAccountDialog();

  @override
  State<_NewAccountDialog> createState() => _NewAccountDialogState();
}

class _NewAccountDialogState extends State<_NewAccountDialog> {
  final _formKey = GlobalKey<FormState>();
  final _iban = TextEditingController();
  Currency? _currency = Currency.EUR;

  @override
  void dispose() {
    _iban.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: BlocBuilder<AccountsBloc, AccountsState>(
          buildWhen: (p, n) => p.submitting != n.submitting || p.submitError != n.submitError,
          builder: (context, state) {
            return Form(
              key: _formKey,
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 320),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    const Text('New Account', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 12),
                    TextFormField(
                      controller: _iban,
                      decoration: const InputDecoration(labelText: 'IBAN (optional)', border: OutlineInputBorder()),
                    ),
                    const SizedBox(height: 12),
                    DropdownButtonFormField<Currency>(
                      value: _currency,
                      items: Currency.values.map((c) => DropdownMenuItem(value: c, child: Text(c.name))).toList(),
                      onChanged: state.submitting ? null : (v) => setState(() => _currency = v),
                      decoration: const InputDecoration(labelText: 'Currency'),
                    ),
                    if (state.submitError != null) ...[
                      const SizedBox(height: 8),
                      Text(state.submitError!, style: const TextStyle(color: Colors.red)),
                    ],
                    const SizedBox(height: 12),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.end,
                      children: [
                        TextButton(
                          onPressed: state.submitting ? null : () => Navigator.of(context).pop(),
                          child: const Text('Cancel'),
                        ),
                        const SizedBox(width: 8),
                        FilledButton(
                          onPressed: state.submitting
                              ? null
                              : () async {
                                  if (!_formKey.currentState!.validate()) return;
                                  final req = CreateAccountRequest(
                                    iban: _iban.text.isEmpty ? null : _iban.text,
                                    currency: _currency,
                                  );
                                  final created = await context.read<AccountsBloc>().create(req);
                                  if (created != null && context.mounted) {
                                    Navigator.of(context).pop();
                                  }
                                },
                          child: state.submitting
                              ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                              : const Text('Create'),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            );
          },
        ),
      ),
    );
  }
}
