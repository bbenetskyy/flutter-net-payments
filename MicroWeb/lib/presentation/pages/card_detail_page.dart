import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/responses/card_response.dart';
import '../../data/models/card_options.dart';
import '../../data/models/requests/update_card_request.dart';
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
      final bool active = card.options?.contains(o) ?? false;
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
  bool _saving = false;
  late int _singleLimit;
  late int _monthlyLimit;
  late Set<CardOptions> _selectedOptions;
  late final TextEditingController _singleCtrl;
  late final TextEditingController _monthlyCtrl;

  @override
  void initState() {
    super.initState();
    _singleLimit = widget.card.singleTransactionLimit;
    _monthlyLimit = widget.card.monthlyLimit;
    _singleCtrl = TextEditingController(text: _singleLimit.toString());
    _monthlyCtrl = TextEditingController(text: _monthlyLimit.toString());
    // If backend returns a single option, initialize set with it (skip None)
    _selectedOptions = <CardOptions>{};
    if (widget.card.options?.isNotEmpty ?? false) {
      _selectedOptions.addAll(widget.card.options!);
    }
  }

  @override
  void dispose() {
    _singleCtrl.dispose();
    _monthlyCtrl.dispose();
    super.dispose();
  }

  Future<void> _print() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Confirm Print'),
        content: const Text('Are you sure you want to print this card? This action should be confirmed by OTP'),
        actions: [
          TextButton(onPressed: () => context.pop(false), child: const Text('Cancel')),
          ElevatedButton(onPressed: () => context.pop(true), child: const Text('Print')),
        ],
      ),
    );
    if (confirm != true) return;
    setState(() => _posting = true);
    try {
      await context.read<CardsBloc>().printCard(widget.card.id);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Card print requested')));
      setState(() => _posting = false);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to print card: $e')));
      setState(() => _posting = false);
    }
  }

  Future<void> _terminate() async {
    final confirm = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Confirm Termination'),
        content: const Text('Are you sure you want to terminate this card? This action should be confirmed by OTP'),
        actions: [
          TextButton(onPressed: () => context.pop(false), child: const Text('Cancel')),
          ElevatedButton(
            onPressed: () => context.pop(true),
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red[700]),
            child: const Text('Terminate'),
          ),
        ],
      ),
    );
    if (confirm != true) return;
    setState(() => _posting = true);
    try {
      await context.read<CardsBloc>().terminate(widget.card.id);
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Card termination requested')));
      context.pop();
      setState(() => _posting = false);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to terminate card: $e')));
      setState(() => _posting = false);
    }
  }

  Future<void> _assign() async {
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

  void _resetFromCard(CardResponse card) {
    setState(() {
      _singleLimit = card.singleTransactionLimit;
      _monthlyLimit = card.monthlyLimit;
      _singleCtrl.text = _singleLimit.toString();
      _monthlyCtrl.text = _monthlyLimit.toString();
      _selectedOptions = <CardOptions>{};
      if (card.options?.isNotEmpty ?? false) {
        _selectedOptions.addAll(card.options!);
      }
    });
  }

  Future<void> _update(CardResponse card) async {
    setState(() => _saving = true);
    try {
      final req = UpdateCardRequest(
        singleTransactionLimit: _singleLimit.toDouble(),
        monthlyLimit: _monthlyLimit.toDouble(),
        options: card.printed ? null : _selectedOptions,
      );
      final updated = await context.read<CardsBloc>().update(card.id, req);
      if (!mounted) return;
      if (updated != null) {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Card updated')));
      } else {
        ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Failed to update card')));
      }
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Failed to update card: $e')));
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final card = context.select<CardsBloc, CardResponse>(
      (b) => b.state.items.firstWhere((e) => e.id == widget.card.id, orElse: () => widget.card),
    );
    final isUnassigned = (card.assignedUserId?.isEmpty ?? true);
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
                    onPressed: _posting ? null : _assign,
                    icon: _posting
                        ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                        : const Icon(Icons.undo),
                    label: const Text('Assign User'),
                  ),
                ],
                if (!card.terminated) ...[
                  const SizedBox(width: 8),
                  FilledButton.icon(
                    onPressed: _posting ? null : _terminate,
                    icon: _posting
                        ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                        : const Icon(Icons.block),
                    label: const Text('Terminate Card'),
                    style: FilledButton.styleFrom(backgroundColor: Colors.red[700]),
                  ),
                ],
                if (!card.printed && !card.terminated) ...[
                  const SizedBox(width: 8),
                  FilledButton.icon(
                    onPressed: _posting ? null : _print,
                    icon: _posting
                        ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                        : const Icon(Icons.print),
                    label: const Text('Print Card'),
                  ),
                ],
                const SizedBox(width: 8),
              ],
            ),
            const SizedBox(height: 12),
            Text('Type: ${card.type.name}'),
            Text('Printed: ${card.printed ? 'Yes' : 'No'}'),
            Text('Terminated: ${card.terminated ? 'Yes' : 'No'}'),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: TextField(
                    readOnly: card.terminated,
                    controller: _singleCtrl,
                    decoration: const InputDecoration(labelText: 'Single transaction limit'),
                    keyboardType: TextInputType.number,
                    onChanged: (v) {
                      final val = int.tryParse(v);
                      if (val != null) setState(() => _singleLimit = val);
                    },
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: TextField(
                    readOnly: card.terminated,
                    controller: _monthlyCtrl,
                    decoration: const InputDecoration(labelText: 'Monthly limit'),
                    keyboardType: TextInputType.number,
                    onChanged: (v) {
                      final val = int.tryParse(v);
                      if (val != null) setState(() => _monthlyLimit = val);
                    },
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            const Text('Options'),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: CardOptions.values
                  .where((o) => o != CardOptions.None)
                  .map(
                    (o) => FilterChip(
                      label: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [Icon(_optionIcon(o), size: 16), const SizedBox(width: 4), Text(o.name)],
                      ),
                      selected: _selectedOptions.contains(o),
                      onSelected: card.printed || card.terminated
                          ? null
                          : (sel) {
                              setState(() {
                                if (sel) {
                                  _selectedOptions.add(o);
                                } else {
                                  _selectedOptions.remove(o);
                                }
                              });
                            },
                    ),
                  )
                  .toList(),
            ),
            const SizedBox(height: 12),
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                TextButton(
                  onPressed: _saving
                      ? null
                      : () {
                          _resetFromCard(card);
                        },
                  child: const Text('Discard'),
                ),
                const SizedBox(width: 8),
                FilledButton.icon(
                  onPressed: _saving
                      ? null
                      : () async {
                          await _update(card);
                        },
                  icon: _saving
                      ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2))
                      : const Icon(Icons.save),
                  label: const Text('Update card'),
                ),
              ],
            ),
            const SizedBox(height: 12),
            if ((card.assignedUserId ?? '').isNotEmpty) Text('Assigned User: ${card.assignedUserId}'),
            Text('Created: ${card.createdAt}'),
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
