import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../logic/cards/cards_bloc.dart';
import '../../data/models/responses/card_response.dart';

class CardsListPage extends StatelessWidget {
  const CardsListPage({super.key});

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
              const SizedBox.shrink(),
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
                        childAspectRatio: 3,
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
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(item.name, style: const TextStyle(fontWeight: FontWeight.bold)),
                    Text(
                      'Type: ${item.type.name}${item.printed ? ' • Printed' : ''}${item.terminated ? ' • Terminated' : ''}',
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
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
