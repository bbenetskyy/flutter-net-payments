import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/responses/user_response.dart';

class UserDetailPage extends StatelessWidget {
  const UserDetailPage({super.key, required this.user});
  final UserResponse user;

  @override
  Widget build(BuildContext context) {
    final title = (user.displayName ?? user.email ?? 'User');
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
                const Icon(Icons.person, size: 28),
                const SizedBox(width: 8),
                Text(title, style: const TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              ],
            ),
            const SizedBox(height: 12),
            if ((user.email ?? '').isNotEmpty) Text(user.email!, style: const TextStyle(color: Colors.grey)),
            const SizedBox(height: 12),
            if ((user.role ?? '').isNotEmpty) Text('Role: ${user.role}'),
            if ((user.createdAt ?? '').isNotEmpty) Text('Created: ${user.createdAt}'),
          ],
        ),
      ),
    );
  }
}
