import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../presentation/pages/dashboard_page.dart';
import '../presentation/pages/splash_page.dart';
import '../presentation/pages/sign_in_page.dart';
import '../presentation/pages/sign_up_page.dart';
import '../presentation/pages/payment_detail_page.dart';
import '../presentation/pages/card_detail_page.dart';
import '../presentation/pages/user_detail_page.dart';
import '../presentation/pages/registration_success_page.dart';
import '../presentation/pages/settings_page.dart';
import '../presentation/pages/profile_page.dart';
import '../presentation/pages/not_found_page.dart';
import '../presentation/pages/payments_list_page.dart';
import '../presentation/pages/cards_list_page.dart';
import '../presentation/pages/users_list_page.dart';
import '../logic/auth/auth_bloc.dart';
import '../logic/items/items_bloc.dart';
import '../data/repositories/in_memory_item_repository.dart';

class AppRouter {
  AppRouter({required AuthBloc authBloc})
    : router = GoRouter(
        initialLocation: '/',
        navigatorKey: _rootKey,
        refreshListenable: GoRouterRefreshStream(authBloc.stream),
        redirect: (context, state) {
          final authState = authBloc.state;
          final isLoggingIn =
              state.matchedLocation == '/signin' ||
              state.matchedLocation == '/signup' ||
              state.matchedLocation == '/registration-success';
          if (authState.status == AuthStatus.unknown) return '/';
          if (authState.status == AuthStatus.unauthenticated) {
            return isLoggingIn ? null : '/signin';
          }
          if (authState.status == AuthStatus.authenticated && isLoggingIn) {
            return '/payments';
          }
          return null;
        },
        routes: [
          GoRoute(path: '/', builder: (_, __) => const SplashPage()),
          GoRoute(path: '/signin', builder: (_, __) => const SignInPage()),
          GoRoute(path: '/signup', builder: (_, __) => const SignUpPage()),
          GoRoute(path: '/registration-success', builder: (_, __) => const RegistrationSuccessPage()),
          ShellRoute(
            builder: (context, state, child) => DashboardPage(shellChild: child),
            routes: [
              GoRoute(
                path: '/payments',
                builder: (context, __) => BlocProvider<ItemsBloc>(
                  create: (ctx) => ItemsBloc(InMemoryItemRepository(seedKey: 'payments'))..add(ItemsRequested()),
                  child: const PaymentsListPage(),
                ),
              ),
              GoRoute(
                path: '/cards',
                builder: (context, __) => BlocProvider<ItemsBloc>(
                  create: (ctx) => ItemsBloc(InMemoryItemRepository(seedKey: 'cards'))..add(ItemsRequested()),
                  child: const CardsListPage(),
                ),
              ),
              GoRoute(
                path: '/users',
                builder: (context, __) => BlocProvider<ItemsBloc>(
                  create: (ctx) => ItemsBloc(InMemoryItemRepository(seedKey: 'users'))..add(ItemsRequested()),
                  child: const UsersListPage(),
                ),
              ),
              GoRoute(
                path: '/payments/:id',
                builder: (ctx, st) => BlocProvider<ItemsBloc>(
                  create: (ctx) => ItemsBloc(InMemoryItemRepository(seedKey: 'payments'))..add(ItemsRequested()),
                  child: PaymentDetailPage(id: st.pathParameters['id']!),
                ),
              ),
              GoRoute(
                path: '/cards/:id',
                builder: (ctx, st) => BlocProvider<ItemsBloc>(
                  create: (ctx) => ItemsBloc(InMemoryItemRepository(seedKey: 'cards'))..add(ItemsRequested()),
                  child: CardDetailPage(id: st.pathParameters['id']!),
                ),
              ),
              GoRoute(
                path: '/users/:id',
                builder: (ctx, st) => BlocProvider<ItemsBloc>(
                  create: (ctx) => ItemsBloc(InMemoryItemRepository(seedKey: 'users'))..add(ItemsRequested()),
                  child: UserDetailPage(id: st.pathParameters['id']!),
                ),
              ),
              GoRoute(path: '/settings', builder: (_, __) => const SettingsPage()),
              GoRoute(path: '/profile', builder: (_, __) => const ProfilePage()),
            ],
          ),
        ],
        errorBuilder: (_, __) => const NotFoundPage(),
      );

  static final _rootKey = GlobalKey<NavigatorState>();
  final GoRouter router;
}

class GoRouterRefreshStream extends ChangeNotifier {
  GoRouterRefreshStream(Stream<dynamic> stream) {
    notifyListeners();
    _sub = stream.asBroadcastStream().listen((_) => notifyListeners());
  }
  late final StreamSubscription<dynamic> _sub;
  @override
  void dispose() {
    _sub.cancel();
    super.dispose();
  }
}
