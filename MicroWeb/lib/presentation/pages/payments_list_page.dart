import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import 'package:flutter/services.dart';
import '../../logic/payments/payments_cubit.dart';
import '../../data/models/responses/payment_response.dart';
import '../../data/models/requests/create_payment_request.dart';
import '../../data/models/currency.dart';

Color _statusColor(String status) {
  final s = status.toLowerCase();
  if (s == 'confirmed') return Colors.green;
  if (s == 'pending') return Colors.amber;
  if (s == 'rejected') return Colors.red;
  return Colors.grey;
}

class PaymentsListPage extends StatelessWidget {
  const PaymentsListPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(16.0),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: [
              Expanded(
                child: BlocBuilder<PaymentsCubit, PaymentsState>(
                  buildWhen: (p, n) => p.currentUser != n.currentUser,
                  builder: (context, state) {
                    final name = state.currentUser?.displayName;
                    final title = (name == null || name.isEmpty) ? 'Payments' : 'Payments — $name';
                    return Text(title, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold));
                  },
                ),
              ),
              FilledButton.icon(
                onPressed: () async {
                  await showDialog(
                    context: context,
                    builder: (_) =>
                        BlocProvider.value(value: context.read<PaymentsCubit>(), child: const _NewPaymentDialog()),
                  );
                },
                icon: const Icon(Icons.add),
                label: const Text('New'),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: BlocBuilder<PaymentsCubit, PaymentsState>(
              builder: (context, state) {
                if (state.loading) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (state.error != null) {
                  return Center(child: Text('Failed to load: ${state.error}'));
                }
                if (state.items.isEmpty) {
                  return const Center(child: Text('No payments yet.'));
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
                        return _PaymentCard(item: item);
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

class _PaymentCard extends StatelessWidget {
  const _PaymentCard({required this.item});
  final PaymentResponse item;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: InkWell(
        onTap: () => context.push('/payments/${item.id}'),
        child: Padding(
          padding: const EdgeInsets.all(12.0),
          child: Row(
            children: [
              const Icon(Icons.account_balance_wallet, size: 36),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(item.beneficiaryName, style: const TextStyle(fontWeight: FontWeight.bold)),
                    Text(item.details ?? '', maxLines: 1, overflow: TextOverflow.ellipsis),
                  ],
                ),
              ),
              const SizedBox(width: 8),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    '${item.amount.toStringAsFixed(2)} ${item.currency}',
                    style: const TextStyle(fontWeight: FontWeight.bold),
                  ),
                  Chip(
                    labelPadding: const EdgeInsets.symmetric(horizontal: 8, vertical: 0),
                    label: Text(item.status),
                    backgroundColor: _statusColor(item.status).withOpacity(0.1),
                    labelStyle: TextStyle(color: _statusColor(item.status)),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _NewPaymentDialog extends StatefulWidget {
  const _NewPaymentDialog();

  @override
  State<_NewPaymentDialog> createState() => _NewPaymentDialogState();
}

class _NewPaymentDialogState extends State<_NewPaymentDialog> {
  final _formKey = GlobalKey<FormState>();
  final _beneficiaryName = TextEditingController();
  final _beneficiaryAccount = TextEditingController();
  final _fromAccount = TextEditingController();
  final _amount = TextEditingController();
  final _details = TextEditingController();
  Currency? _currency = Currency.EUR;

  @override
  void dispose() {
    _beneficiaryName.dispose();
    _beneficiaryAccount.dispose();
    _fromAccount.dispose();
    _amount.dispose();
    _details.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: BlocBuilder<PaymentsCubit, PaymentsState>(
          buildWhen: (p, n) => p.submitting != n.submitting || p.submitError != n.submitError,
          builder: (context, state) {
            return Form(
              key: _formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const Text('New Payment', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _beneficiaryName,
                    decoration: const InputDecoration(labelText: 'Beneficiary Name'),
                    validator: (v) => (v == null || v.isEmpty) ? 'Required' : null,
                  ),
                  TextFormField(
                    controller: _beneficiaryAccount,
                    decoration: const InputDecoration(labelText: 'Beneficiary Account (IBAN)'),
                    validator: (v) => (v == null || v.isEmpty) ? 'Required' : null,
                  ),
                  TextFormField(
                    controller: _fromAccount,
                    decoration: const InputDecoration(labelText: 'From Account (IBAN)'),
                    validator: (v) => (v == null || v.isEmpty) ? 'Required' : null,
                  ),
                  TextFormField(
                    controller: _amount,
                    decoration: const InputDecoration(labelText: 'Amount'),
                    keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    inputFormatters: [FilteringTextInputFormatter.allow(RegExp(r'[0-9\.]'))],
                    validator: (v) {
                      if (v == null || v.isEmpty) return 'Required';
                      final d = double.tryParse(v);
                      if (d == null || d <= 0) return 'Enter a positive number';
                      return null;
                    },
                  ),
                  DropdownButtonFormField<Currency>(
                    value: _currency,
                    items: Currency.values.map((c) => DropdownMenuItem(value: c, child: Text(c.name))).toList(),
                    onChanged: state.submitting ? null : (v) => setState(() => _currency = v),
                    decoration: const InputDecoration(labelText: 'Currency'),
                  ),
                  TextFormField(
                    controller: _details,
                    decoration: const InputDecoration(labelText: 'Details'),
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
                                final req = CreatePaymentRequest(
                                  beneficiaryName: _beneficiaryName.text,
                                  beneficiaryAccount: _beneficiaryAccount.text,
                                  fromAccount: _fromAccount.text,
                                  amount: double.tryParse(_amount.text),
                                  currency: _currency,
                                  details: _details.text.isEmpty ? null : _details.text,
                                );
                                final created = await context.read<PaymentsCubit>().create(req);
                                if (created != null && context.mounted) {
                                  Navigator.of(context).pop();
                                }
                              },
                        child: state.submitting
                            ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                            : const Text('Submit'),
                      ),
                    ],
                  ),
                ],
              ),
            );
          },
        ),
      ),
    );
  }
}
