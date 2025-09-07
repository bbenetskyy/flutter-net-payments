import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../presentation/pages/dashboard_page.dart';
import '../presentation/pages/splash_page.dart';
import '../presentation/pages/sign_in_page.dart';
import '../presentation/pages/sign_up_page.dart';
import '../presentation/pages/payment_detail_page.dart';
import '../data/models/responses/payment_response.dart';
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
import '../logic/cards/cards_bloc.dart';
import '../data/repositories/cards_repository.dart';
import '../data/models/responses/card_response.dart';
import '../logic/payments/payments_bloc.dart';
import '../data/repositories/payments_repository.dart';
import '../data/repositories/users_repository.dart';
import '../data/repositories/accounts_repository.dart';
import '../logic/users/users_bloc.dart';
import '../data/models/responses/user_response.dart';

class AppRouter {
  AppRouter({required AuthBloc authBloc})
    : router = GoRouter(
        initialLocation: '/',
        navigatorKey: _rootKey,
        refreshListenable: GoRouterRefreshStream(authBloc.stream),
        redirect: (context, state) {
          final authState = authBloc.state;
          final isLoggingIn =
              state.matchedLocation == '/' ||
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
                builder: (context, __) => BlocProvider<PaymentsBloc>(
                  create: (ctx) => PaymentsBloc(
                    ctx.read<PaymentsRepository>(),
                    ctx.read<UsersRepository>(),
                    ctx.read<AccountsRepository>(),
                  )..load(),
                  child: const PaymentsListPage(),
                ),
              ),
              GoRoute(
                path: '/cards',
                builder: (context, __) => BlocProvider<CardsBloc>(
                  create: (ctx) => CardsBloc(ctx.read<CardsRepository>())..add(const CardsRequested()),
                  child: const CardsListPage(),
                ),
              ),
              GoRoute(
                path: '/users',
                builder: (context, __) => BlocProvider<UsersBloc>(
                  create: (ctx) => UsersBloc(ctx.read<UsersRepository>())..load(),
                  child: const UsersListPage(),
                ),
              ),
              GoRoute(
                path: '/payments/:id',
                builder: (ctx, st) {
                  final item = st.extra as PaymentResponse?;
                  if (item == null) {
                    return const NotFoundPage();
                  }
                  return PaymentDetailPage(item: item);
                },
              ),
              GoRoute(
                path: '/cards/:id',
                builder: (ctx, st) {
                  final card = st.extra as CardResponse?;
                  if (card == null) return const NotFoundPage();
                  return CardDetailPage(card: card);
                },
              ),
              GoRoute(
                path: '/users/:id',
                builder: (ctx, st) {
                  final user = st.extra as UserResponse?;
                  if (user == null) return const NotFoundPage();
                  return UserDetailPage(user: user);
                },
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
