import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../../logic/auth/auth_bloc.dart';
import '../../logic/theme/theme_cubit.dart';

class AppScaffold extends StatefulWidget {
  const AppScaffold({super.key, required this.child});
  final Widget child;

  @override
  State<AppScaffold> createState() => _AppScaffoldState();
}

class _AppScaffoldState extends State<AppScaffold> {
  int _selectedIndex = 0;

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
                      PopupMenuItem(child: const Text('Profile'), onTap: () => context.push('/profile')),
                      PopupMenuItem(child: const Text('Settings'), onTap: () => context.push('/settings')),
                      PopupMenuItem(
                        child: const Text('Sign out'),
                        onTap: () => context.read<AuthBloc>().add(SignOutRequested()),
                      ),
                    ],
                    child: Row(
                      children: [
                        const CircleAvatar(child: Icon(Icons.person)),
                        const SizedBox(width: 8),
                        Text(state.user?.displayName ?? 'User'),
                      ],
                    ),
                  ),
                );
              }
              return TextButton(onPressed: () => context.go('/signin'), child: const Text('Sign in'));
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
              leading: const Icon(Icons.account_balance_wallet_outlined),
              title: const Text('Payments'),
              onTap: () => context.go('/payments'),
            ),
            ListTile(
              leading: const Icon(Icons.credit_card),
              title: const Text('Cards'),
              onTap: () => context.go('/cards'),
            ),
            ListTile(
              leading: const Icon(Icons.people_alt_outlined),
              title: const Text('Users'),
              onTap: () => context.go('/users'),
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
              selectedIndex: _selectedIndex,
              onDestinationSelected: (i) {
                switch (i) {
                  case 0:
                    context.go('/payments');
                    break;
                  case 1:
                    context.go('/cards');
                    break;
                  case 2:
                    context.go('/users');
                    break;
                  case 3:
                    context.go('/settings');
                    break;
                }
                setState(() {
                  _selectedIndex = i;
                });
              },
              destinations: const [
                NavigationRailDestination(icon: Icon(Icons.account_balance_wallet_outlined), label: Text('Payments')),
                NavigationRailDestination(icon: Icon(Icons.credit_card), label: Text('Cards')),
                NavigationRailDestination(icon: Icon(Icons.people_alt_outlined), label: Text('Users')),
                NavigationRailDestination(icon: Icon(Icons.settings_outlined), label: Text('Settings')),
              ],
            ),
          Expanded(child: widget.child),
        ],
      ),
    );
  }
}
