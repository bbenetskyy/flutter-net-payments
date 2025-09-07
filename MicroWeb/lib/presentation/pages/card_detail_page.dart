import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/responses/card_response.dart';
import '../../data/models/card_options.dart';
import '../../data/models/responses/user_response.dart';
import '../../logic/cards/cards_bloc.dart';

IconData _optionIcon(CardOptions o) {
  switch (o) {
    case CardOptions.ATM:
      return Icons.atm;
    case CardOptions.MagneticStripeReader:
      return Icons.credit_card;
    case CardOptions.Contactless:
      return Icons.nfc;
    case CardOptions.OnlinePayments:
      return Icons.shopping_cart;
    case CardOptions.AllowChangingSettings:
      return Icons.settings;
    case CardOptions.AllowPlasticOrder:
      return Icons.add_card;
    case CardOptions.None:
      return Icons.block;
  }
}

Widget _buildOptionsWrap(CardResponse card) {
  final all = CardOptions.values.where((o) => o != CardOptions.None).toList();
  return Wrap(
    spacing: 8,
    runSpacing: 8,
    children: all.map((o) {
      final bool active = (card.options == o);
      final color = active ? Colors.black87 : Colors.grey;
      return Row(
        children: [
          Icon(_optionIcon(o), size: 20, color: color),
          const SizedBox(width: 4),
          Text(o.name, style: TextStyle(color: color)),
        ],
      );
    }).toList(),
  );
}

class CardDetailPage extends StatefulWidget {
  const CardDetailPage({super.key, required this.card});
  final CardResponse card;

  @override
  State<CardDetailPage> createState() => _CardDetailPageState();
}

class _CardDetailPageState extends State<CardDetailPage> {
  bool _posting = false;

  Future<void> _assignUser() async {
    setState(() => _posting = true);
    try {
      await showDialog(
        context: context,
        builder: (_) => BlocProvider.value(
          value: context.read<CardsBloc>(),
          child: _AssignCardDialog(cardId: widget.card.id),
        ),
      );
      setState(() => _posting = false);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to assign user: $e')));
      setState(() => _posting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final isUnassigned = widget.card.assignedUserId?.isEmpty ?? true;
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
                const Icon(Icons.credit_card, size: 28),
                const SizedBox(width: 8),
                Chip(
                  label: Text(
                    isUnassigned ? 'Unassigned' : widget.card.name,
                    style: TextStyle(color: isUnassigned ? Colors.red[800] : Colors.green[800]),
                  ),
                  backgroundColor: isUnassigned ? Colors.red[100] : Colors.green[100],
                ),
                const Spacer(),
                if (isUnassigned) ...[
                  const SizedBox(width: 8),
                  FilledButton.icon(
                    onPressed: _posting ? null : _assignUser,
                    icon: _posting
                        ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                        : const Icon(Icons.undo),
                    label: const Text('Assign User'),
                  ),
                ],
                const SizedBox(width: 8),
              ],
            ),
            const SizedBox(height: 12),
            Text('Type: ${widget.card.type.name}'),
            Text('Printed: ${widget.card.printed ? 'Yes' : 'No'}'),
            Text('Terminated: ${widget.card.terminated ? 'Yes' : 'No'}'),
            Text('Single Tx Limit: ${widget.card.singleTransactionLimit}'),
            Text('Monthly Limit: ${widget.card.monthlyLimit}'),
            const SizedBox(height: 12),
            const Text('Options'),
            const SizedBox(height: 8),
            _buildOptionsWrap(widget.card),
            const SizedBox(height: 12),
            if ((widget.card.assignedUserId ?? '').isNotEmpty) Text('Assigned User: ${widget.card.assignedUserId}'),
            Text('Created: ${widget.card.createdAt}'),
          ],
        ),
      ),
    );
  }
}

class _AssignCardDialog extends StatefulWidget {
  const _AssignCardDialog({required this.cardId});
  final String cardId;

  @override
  State<_AssignCardDialog> createState() => _AssignCardDialogState();
}

class _AssignCardDialogState extends State<_AssignCardDialog> {
  final _formKey = GlobalKey<FormState>();
  bool _posting = false;
  String? _selectedUserId; // selected from /users

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;
    setState(() => _posting = true);
    try {
      await context.read<CardsBloc>().assignUser(widget.cardId, _selectedUserId!);
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Awaiting user assignment...')));
      if (!mounted) return;
      context.pop();
    } catch (e) {
      if (!mounted) return;
      context.pop();
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to assign user: $e')));
      setState(() => _posting = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Assign Card to User'),
      content: BlocBuilder<CardsBloc, CardsState>(
        builder: (context, state) {
          return Form(
            key: _formKey,
            child: DropdownButtonFormField<String>(
              value: _selectedUserId,
              hint: const Text('Select User (optional)'),
              items: state.users.where((u) => ((u.displayName ?? u.email ?? '').isNotEmpty)).map((u) {
                final name = (u.displayName ?? u.email) ?? '';
                return DropdownMenuItem<String>(value: u.id, child: Text(name));
              }).toList(),
              onChanged: (userId) async {
                setState(() {
                  _selectedUserId = userId;
                });
              },
              validator: (value) {
                if (value == null || value.isEmpty) {
                  return 'Please select a user';
                }
                return null;
              },
              decoration: const InputDecoration(labelText: 'Beneficiary (from users)'),
            ),
          );
        },
      ),
      actions: [
        TextButton(onPressed: _posting ? null : () => context.pop(), child: const Text('Cancel')),
        ElevatedButton(
          onPressed: _posting ? null : _submit,
          child: _posting ? const CircularProgressIndicator() : const Text('Assign'),
        ),
      ],
    );
  }
}
