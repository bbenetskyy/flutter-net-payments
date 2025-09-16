import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../logic/payments/payments_bloc.dart';
import '../../data/models/responses/payment_response.dart';

Color _statusColor(String status) {
  final s = status.toLowerCase();
  if (s == 'confirmed' || s == 'reverted') return Colors.green;
  if (s == 'pending' || s == 'awaitingreversion') return Colors.amber;
  if (s == 'rejected' || s == 'reversionrejected') return Colors.red;
  return Colors.grey;
}

bool _isCompleted(String status) {
  final s = status.toLowerCase();
  return s == 'confirmed' || s == 'reverted' || s.contains('success');
}

class PaymentDetailPage extends StatefulWidget {
  const PaymentDetailPage({super.key, required this.item});
  final PaymentResponse item;

  @override
  State<PaymentDetailPage> createState() => _PaymentDetailPageState();
}

class _PaymentDetailPageState extends State<PaymentDetailPage> {
  bool _posting = false;
  bool _changed = false;

  Future<void> _revert() async {
    setState(() => _posting = true);
    try {
      await context.read<PaymentsBloc>().revert(widget.item.id);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Revert verification submitted')));
      setState(() => _posting = false);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to submit revert: $e')));
      setState(() => _posting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final item = context.select<PaymentsBloc, PaymentResponse>(
      (b) => b.state.items.firstWhere((e) => e.id == widget.item.id, orElse: () => widget.item),
    );
    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, result) {
        if (didPop) return;
        context.pop(_changed);
      },
      child: Scaffold(
        body: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  IconButton(onPressed: () => context.pop(_changed), icon: const Icon(Icons.arrow_back)),
                  const SizedBox(width: 8),
                  const Icon(Icons.account_balance_wallet, size: 28),
                  const SizedBox(width: 8),
                  Text(item.beneficiaryName, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
                  const Spacer(),
                  if (_isCompleted(item.status)) ...[
                    FilledButton.icon(
                      onPressed: _posting ? null : _revert,
                      icon: _posting
                          ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                          : const Icon(Icons.undo),
                      label: const Text('Revert'),
                    ),
                    const SizedBox(width: 8),
                  ],
                  Chip(
                    label: Text(item.status),
                    backgroundColor: _statusColor(item.status).withOpacity(0.7),
                    labelStyle: TextStyle(color: _statusColor(item.status)),
                  ),
                  const SizedBox(width: 8),
                  Text(
                    '${item.amount.toStringAsFixed(2)} ${item.currency}',
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              if ((item.details ?? '').isNotEmpty) Text(item.details!, style: const TextStyle(color: Colors.grey)),
              const SizedBox(height: 12),
              Text('From: ${item.fromAccount}'),
              Text('To: ${item.beneficiaryAccount}'),
              Text('Status: ${item.status}'),
              Text('Created: ${item.createdAt}'),
            ],
          ),
        ),
      ),
    );
  }
}
