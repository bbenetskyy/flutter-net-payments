import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';

import '../../data/models/responses/account_response.dart';
import '../../logic/accounts/accounts_bloc.dart';

class AccountDetailPage extends StatefulWidget {
  const AccountDetailPage({super.key, required this.item});
  final AccountResponse item;

  @override
  State<AccountDetailPage> createState() => _AccountDetailPageState();
}

class _AccountDetailPageState extends State<AccountDetailPage> {
  bool _posting = false;
  bool _changed = false;

  Future<void> _delete() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Delete Account'),
        content: const Text('Are you sure you want to delete this account?'),
        actions: [
          TextButton(onPressed: () => context.pop(false), child: const Text('Cancel')),
          FilledButton(
            onPressed: () => context.pop(true),
            style: FilledButton.styleFrom(backgroundColor: Colors.red),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
    if (confirm != true) return;
    setState(() => _posting = true);
    try {
      await context.read<AccountsBloc>().delete(widget.item.id ?? '');
      if (!mounted) return;
      _changed = true;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Account deleted')));
      context.pop(true);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to delete: $e')));
      setState(() => _posting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final item = context.select<AccountsBloc, AccountResponse>(
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
                  const Icon(Icons.account_balance, size: 28),
                  const SizedBox(width: 8),
                  Text(item.iban ?? '(no IBAN)', style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
                  const Spacer(),
                  FilledButton.icon(
                    onPressed: _posting ? null : _delete,
                    icon: _posting
                        ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                        : const Icon(Icons.delete),
                    label: const Text('Delete'),
                    style: FilledButton.styleFrom(backgroundColor: Colors.red[700]),
                  ),
                  const SizedBox(width: 8),
                ],
              ),
              const SizedBox(height: 12),
              if (item.currency != null) Text('Currency: ${item.currency!.name}'),
              if (item.createdAt != null) Text('Created: ${item.createdAt}'),
            ],
          ),
        ),
      ),
    );
  }
}
