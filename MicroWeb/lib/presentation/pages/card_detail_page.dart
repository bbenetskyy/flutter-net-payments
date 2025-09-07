import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/responses/card_response.dart';

class CardDetailPage extends StatelessWidget {
  const CardDetailPage({super.key, required this.card});
  final CardResponse card;

  @override
  Widget build(BuildContext context) {
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
                Text(card.name, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              ],
            ),
            const SizedBox(height: 12),
            Text('Type: ${card.type.name}'),
            Text('Printed: ${card.printed ? 'Yes' : 'No'}'),
            Text('Terminated: ${card.terminated ? 'Yes' : 'No'}'),
            Text('Single Tx Limit: ${card.singleTransactionLimit}'),
            Text('Monthly Limit: ${card.monthlyLimit}'),
            if ((card.assignedUserId ?? '').isNotEmpty) Text('Assigned User: ${card.assignedUserId}'),
            Text('Created: ${card.createdAt}'),
          ],
        ),
      ),
    );
  }
}
