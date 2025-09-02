import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../logic/auth/auth_bloc.dart';
import '../../logic/theme/theme_cubit.dart';

class AppScaffold extends StatelessWidget {
  const AppScaffold({super.key, required this.child});
  final Widget child;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('My Web App'),
        actions: [
          IconButton(
            onPressed: () => context.read<ThemeCubit>().toggle(),
            icon: const Icon(Icons.brightness_6),
            tooltip: 'Toggle theme',
          ),
          BlocBuilder<AuthBloc, AuthState>(
            builder: (context, state) {
              if (state.status == AuthStatus.authenticated) {
                return Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 8.0),
                  child: PopupMenuButton(
                    tooltip: 'Account',
                    itemBuilder: (_) => [
                      PopupMenuItem(
                        child: const Text('Profile'),
                        onTap: () => context.push('/profile'),
                      ),
                      PopupMenuItem(
                        child: const Text('Settings'),
                        onTap: () => context.push('/settings'),
                      ),
                      PopupMenuItem(
                        child: const Text('Sign out'),
                        onTap: () => context.read<AuthBloc>().add(SignOutRequested()),
                      ),
                    ],
                    child: Row(children: [
                      const CircleAvatar(child: Icon(Icons.person)),
                      const SizedBox(width: 8),
                      Text(state.user?.displayName ?? 'User'),
                    ]),
                  ),
                );
              }
              return TextButton(
                onPressed: () => context.go('/signin'),
                child: const Text('Sign in'),
              );
            },
          ),
        ],
      ),
      drawer: Drawer(
        child: ListView(
          padding: EdgeInsets.zero,
          children: [
            const DrawerHeader(child: Text('Navigation')),
            ListTile(
              leading: const Icon(Icons.dashboard),
              title: const Text('Dashboard'),
              onTap: () => context.go('/dashboard'),
            ),
            ListTile(
              leading: const Icon(Icons.list),
              title: const Text('Items'),
              onTap: () => context.go('/items'),
            ),
            ListTile(
              leading: const Icon(Icons.settings),
              title: const Text('Settings'),
              onTap: () => context.go('/settings'),
            ),
          ],
        ),
      ),
      body: Row(
        children: [
          if (MediaQuery.of(context).size.width >= 1100)
            NavigationRail(
              selectedIndex: 0,
              onDestinationSelected: (i) {
                switch (i) {
                  case 0:
                    context.go('/dashboard');
                    break;
                  case 1:
                    context.go('/items');
                    break;
                  case 2:
                    context.go('/settings');
                    break;
                }
              },
              destinations: const [
                NavigationRailDestination(icon: Icon(Icons.dashboard_outlined), label: Text('Dashboard')),
                NavigationRailDestination(icon: Icon(Icons.list_alt), label: Text('Items')),
                NavigationRailDestination(icon: Icon(Icons.settings_outlined), label: Text('Settings')),
              ],
            ),
          Expanded(child: child),
        ],
      ),
    );
  }
}
