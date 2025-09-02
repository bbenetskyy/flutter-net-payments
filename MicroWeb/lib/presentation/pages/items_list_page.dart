import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../logic/items/items_bloc.dart';
import '../../data/models/item.dart';

class ItemsListPage extends StatelessWidget {
  const ItemsListPage({super.key});

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
                child: Text('Items', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              ),
              FilledButton.icon(
                onPressed: () {
                  final newItem = Item(id: DateTime.now().millisecondsSinceEpoch.toString(), title: 'New', subtitle: 'Draft');
                  context.push('/items/${newItem.id}/edit', extra: newItem);
                },
                icon: const Icon(Icons.add),
                label: const Text('New'),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: BlocBuilder<ItemsBloc, ItemsState>(
              builder: (context, state) {
                if (state.loading) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (state.items.isEmpty) {
                  return const Center(child: Text('No items yet.'));
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
                        return _ItemCard(item: item);
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

class _ItemCard extends StatelessWidget {
  const _ItemCard({required this.item});
  final Item item;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: InkWell(
        onTap: () => context.push('/items/${item.id}'),
        child: Padding(
          padding: const EdgeInsets.all(12.0),
          child: Row(
            children: [
              const Icon(Icons.description, size: 36),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(item.title, style: const TextStyle(fontWeight: FontWeight.bold)),
                    Text(item.subtitle, maxLines: 1, overflow: TextOverflow.ellipsis),
                  ],
                ),
              ),
              IconButton(
                tooltip: 'Edit',
                onPressed: () => context.push('/items/${item.id}/edit', extra: item),
                icon: const Icon(Icons.edit),
              ),
              IconButton(
                tooltip: 'Delete',
                onPressed: () => context.read<ItemsBloc>().add(ItemDeleted(item.id)),
                icon: const Icon(Icons.delete_outline),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
