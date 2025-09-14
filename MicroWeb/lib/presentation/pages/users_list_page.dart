import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../data/models/responses/role_response.dart';
import '../../logic/users/users_bloc.dart';
import '../../data/models/responses/user_response.dart';
import '../../logic/users/users_cubit.dart';

class UsersListPage extends StatelessWidget {
  const UsersListPage({super.key});

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
                child: Text('Users', style: TextStyle(fontSize: 24, fontWeight: FontWeight.bold)),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Expanded(
            child: BlocBuilder<UsersBloc, UsersState>(
              builder: (context, state) {
                if (state.loading) {
                  return const Center(child: CircularProgressIndicator());
                }
                if (state.users.isEmpty) {
                  return const Center(child: Text('No users yet.'));
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
                      itemCount: state.users.length,
                      itemBuilder: (ctx, i) {
                        final user = state.users[i];
                        return _UserCard(user: user, roles: state.roles);
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

class _UserCard extends StatelessWidget {
  const _UserCard({required this.user, required this.roles});
  final List<RoleResponse> roles;
  final UserResponse user;

  @override
  Widget build(BuildContext context) {
    return Card(
      child: InkWell(
        onTap: () => context.push('/users/${user.id}', extra: {user, roles}),
        child: Padding(
          padding: const EdgeInsets.all(12.0),
          child: Row(
            children: [
              const Icon(Icons.person, size: 36),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Text(
                      (user.displayName ?? user.email ?? 'Unnamed'),
                      style: const TextStyle(fontWeight: FontWeight.bold),
                    ),
                    if ((user.email ?? '').isNotEmpty) Text(user.email!, maxLines: 1, overflow: TextOverflow.ellipsis),
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
