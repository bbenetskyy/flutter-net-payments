import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../logic/items/items_bloc.dart';
import '../../data/models/item.dart';

class CardDetailPage extends StatelessWidget {
  const CardDetailPage({super.key, required this.id});
  final String id;

  @override
  Widget build(BuildContext context) {
    final Item item = context.select<ItemsBloc, Item>((b) => b.state.items.firstWhere((e) => e.id == id));
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
                Text(item.title, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              ],
            ),
            const SizedBox(height: 12),
            if (item.subtitle.isNotEmpty) Text(item.subtitle, style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 12),
            if (item.description.isNotEmpty) Text(item.description),
          ],
        ),
      ),
    );
  }
}
