import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/card_type.dart';
import '../../data/models/requests/create_card_request.dart';
import '../../logic/cards/cards_bloc.dart';
import '../../data/models/responses/card_response.dart';
import '../../data/models/card_options.dart';

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
      return Icon(_optionIcon(o), size: 18, color: color);
    }).toList(),
  );
}

class CardsListPage extends StatefulWidget {
  const CardsListPage({super.key});

  @override
  State<CardsListPage> createState() => _CardsListPageState();
}

class _CardsListPageState extends State<CardsListPage> {
  @override
  void initState() {
    super.initState();
    context.read<CardsBloc>().add(const UsersLoadRequested());
  }

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
                child: Text('Cards', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              ),
              FilledButton.icon(
                onPressed: () async {
                  await showDialog(
                    context: context,
                    builder: (_) => BlocProvider.value(value: context.read<CardsBloc>(), child: const _NewCardDialog()),
                  );
                },
                icon: const Icon(Icons.add),
                label: const Text('New'),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: BlocBuilder<CardsBloc, CardsState>(
              builder: (context, state) {
                if (state.loading) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (state.items.isEmpty) {
                  return const Center(child: Text('No cards yet.'));
                }
                return LayoutBuilder(
                  builder: (ctx, constraints) {
                    final crossAxis = constraints.maxWidth ~/ 320;
                    return GridView.builder(
                      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: crossAxis.clamp(1, 4),
                        childAspectRatio: 2,
                        crossAxisSpacing: 12,
                        mainAxisSpacing: 12,
                      ),
                      itemCount: state.items.length,
                      itemBuilder: (ctx, i) {
                        final item = state.items[i];
                        return _CardCard(item: item);
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

class _CardCard extends StatelessWidget {
  const _CardCard({required this.item});
  final CardResponse item;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: InkWell(
        onTap: () => context.push('/cards/${item.id}', extra: item),
        child: Padding(
          padding: const EdgeInsets.all(12.0),
          child: Row(
            children: [
              const Icon(Icons.credit_card, size: 36),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(item.name, style: const TextStyle(fontWeight: FontWeight.bold)),
                    Text(
                      'Type: ${item.type.name}${item.printed ? ' • Printed' : ''}${item.terminated ? ' • Terminated' : ''}',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 8),
                    if (item.assignedUserId == null || item.assignedUserId!.isEmpty)
                      Chip(
                        label: Text('Unassigned', style: TextStyle(color: Colors.red[800])),
                        backgroundColor: Colors.red[100],
                      ),
                    const SizedBox(height: 8),
                    _buildOptionsWrap(item),
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

class _NewCardDialog extends StatefulWidget {
  const _NewCardDialog();

  @override
  State<_NewCardDialog> createState() => _NewCardDialogState();
}

class _NewCardDialogState extends State<_NewCardDialog> {
  final _formKey = GlobalKey<FormState>();
  String _name = '';
  int _singleTransactionLimit = 0;
  int _monthlyLimit = 0;
  CardType _type = CardType.Personal;

  @override
  Widget build(BuildContext context) {
    return Dialog(
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: BlocBuilder<CardsBloc, CardsState>(
          buildWhen: (previous, current) => previous.loading != current.loading,
          builder: (context, state) {
            return Form(
              key: _formKey,
              child: ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 280),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Text('New Card', style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold)),
                    const SizedBox(height: 12),
                    DropdownButtonFormField<CardType>(
                      decoration: const InputDecoration(labelText: 'Type', border: OutlineInputBorder()),
                      value: _type,
                      items: CardType.values
                          .map((type) => DropdownMenuItem(value: type, child: Text(type.name)))
                          .toList(),
                      onChanged: (value) {
                        if (value != null) {
                          setState(() {
                            _type = value;
                          });
                        }
                      },
                    ),
                    const SizedBox(height: 12),
                    if (_type == CardType.Shared)
                      TextFormField(
                        decoration: const InputDecoration(labelText: 'Name', border: OutlineInputBorder()),
                        onChanged: (value) => _name = value,
                        validator: (value) => (value == null || value.isEmpty) ? 'Please enter Card Name' : null,
                      ),
                    const SizedBox(height: 12),
                    TextFormField(
                      decoration: const InputDecoration(
                        labelText: 'Single Transaction Limit',
                        border: OutlineInputBorder(),
                      ),
                      keyboardType: TextInputType.number,
                      onChanged: (value) => _singleTransactionLimit = int.tryParse(value) ?? 0,
                      validator: (value) {
                        final v = int.tryParse(value ?? '');
                        if (v == null) {
                          return 'Please enter a valid number';
                        }
                        if (v <= 0) {
                          return 'Limit must be greater than zero';
                        }
                        if (v > _monthlyLimit && _monthlyLimit > 0) {
                          return 'Single transaction limit must be less than monthly limit';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 12),
                    TextFormField(
                      decoration: const InputDecoration(labelText: 'Monthly Limit', border: OutlineInputBorder()),
                      keyboardType: TextInputType.number,
                      onChanged: (value) => _monthlyLimit = int.tryParse(value) ?? 0,
                      validator: (value) {
                        final v = int.tryParse(value ?? '');
                        if (v == null) {
                          return 'Please enter a valid number';
                        }
                        if (v <= 0) {
                          return 'Limit must be greater than zero';
                        }
                        if (v < _singleTransactionLimit) {
                          return 'Monthly limit must be greater than single transaction limit';
                        }
                        return null;
                      },
                    ),
                    const SizedBox(height: 12),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.end,
                      children: [
                        TextButton(
                          onPressed: state.loading ? null : () => Navigator.of(context).pop(),
                          child: const Text('Cancel'),
                        ),
                        const SizedBox(width: 8),
                        FilledButton(
                          onPressed: () async {
                            if (_formKey.currentState?.validate() ?? false) {
                              if (_type == CardType.Personal) {
                                _name = '';
                              }
                              final req = CreateCardRequest(
                                type: _type,
                                name: _name,
                                singleTransactionLimit: _singleTransactionLimit,
                                monthlyLimit: _monthlyLimit,
                              );
                              final created = await context.read<CardsBloc>().create(req);
                              if (created != null && context.mounted) {
                                Navigator.of(context).pop();
                              }
                            }
                          },
                          child: const Text('Create'),
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
