import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/requests/create_verification_request.dart';
import '../../data/models/verification_action.dart';
import '../../data/repositories/payments_repository.dart';
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

  Future<void> _revert() async {
    setState(() => _posting = true);
    try {
      final repo = context.read<PaymentsRepository>();
      final req = CreateVerificationRequest(action: VerificationAction.PaymentReverted, targetId: widget.item.id);
      await repo.createPaymentVerification(req);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Revert verification submitted')));
      context.pop();
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to submit revert: $e')));
      setState(() => _posting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final item = widget.item;
    return Scaffold(
      body: Padding(
        padding: const EdgeInsets.all(24.0),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                IconButton(onPressed: () => context.pop(), icon: const Icon(Icons.arrow_back)),
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
    );
  }
}
