import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../logic/payments/payments_cubit.dart';
import '../../data/models/responses/payment_response.dart';

Color _statusColor(String status) {
  final s = status.toLowerCase();
  if (s == 'confirmed') return Colors.green;
  if (s == 'pending') return Colors.amber;
  if (s == 'rejected') return Colors.red;
  return Colors.grey;
}

class PaymentDetailPage extends StatefulWidget {
  const PaymentDetailPage({super.key, required this.id});
  final String id;

  @override
  State<PaymentDetailPage> createState() => _PaymentDetailPageState();
}

class _PaymentDetailPageState extends State<PaymentDetailPage> {
  @override
  void initState() {
    super.initState();
    // Ensure the payment is loaded (in case it's not in the list state)
    // Schedule after first frame to safely access context
    WidgetsBinding.instance.addPostFrameCallback((_) {
      context.read<PaymentsCubit>().loadOne(widget.id);
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = context.watch<PaymentsCubit>().state;
    PaymentResponse? item;
    for (final e in state.items) {
      if (e.id == widget.id) {
        item = e;
        break;
      }
    }

    return Scaffold(
      body: Padding(
        padding: const EdgeInsets.all(24.0),
        child: item == null
            ? const Center(child: CircularProgressIndicator())
            : Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      IconButton(onPressed: () => context.pop(), icon: const Icon(Icons.arrow_back)),
                      const SizedBox(width: 8),
                      const Icon(Icons.account_balance_wallet, size: 28),
                      const SizedBox(width: 8),
                      Text(item!.beneficiaryName, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
                      const Spacer(),
                      Chip(
                        label: Text(item!.status),
                        backgroundColor: _statusColor(item!.status).withOpacity(0.7),
                        labelStyle: TextStyle(color: _statusColor(item!.status)),
                      ),
                      const SizedBox(width: 8),
                      Text(
                        '${item!.amount.toStringAsFixed(2)} ${item!.currency}',
                        style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  if ((item!.details ?? '').isNotEmpty)
                    Text(item!.details!, style: const TextStyle(color: Colors.grey)),
                  const SizedBox(height: 12),
                  Text('From: ${item!.fromAccount}'),
                  Text('To: ${item!.beneficiaryAccount}'),
                  Text('Status: ${item!.status}'),
                  Text('Created: ${item!.createdAt}'),
                ],
              ),
      ),
    );
  }
}
