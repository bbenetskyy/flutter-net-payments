import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'core/theme.dart';
import 'core/app_router.dart';
import 'data/repositories/auth_repository.dart';
import 'data/repositories/item_repository.dart';
import 'data/repositories/rest_auth_repository.dart';
import 'data/repositories/rest_payments_repository.dart';
import 'data/repositories/payments_repository.dart';
import 'data/repositories/users_repository.dart';
import 'data/repositories/rest_users_repository.dart';
import 'data/repositories/cards_repository.dart';
import 'data/repositories/rest_cards_repository.dart';
import 'data/repositories/wallet_repository.dart';
import 'data/repositories/rest_wallet_repository.dart';
import 'data/services/api_client.dart';
import 'data/services/local_storage.dart';
import 'logic/auth/auth_bloc.dart';
import 'logic/items/items_bloc.dart';
import 'logic/theme/theme_cubit.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const AppBootstrap());
}

class AppBootstrap extends StatelessWidget {
  const AppBootstrap({super.key});

  @override
  Widget build(BuildContext context) {
    final storage = TokenStorage();
    final api = ApiClient(baseUrl: 'http://localhost:5247', readAccessToken: storage.readAccess);
    final authRepo = RestAuthRepository(api, storage);
    final itemRepo = RestPaymentsRepository(api);
    final usersRepo = RestUsersRepository(api);
    final cardsRepo = RestCardsRepository(api);
    final walletRepo = RestWalletRepository(api);
    return MultiRepositoryProvider(
      providers: [
        RepositoryProvider<AuthRepository>.value(value: authRepo),
        RepositoryProvider<ItemRepository>.value(value: itemRepo),
        RepositoryProvider<PaymentsRepository>.value(value: itemRepo),
        RepositoryProvider<UsersRepository>.value(value: usersRepo),
        RepositoryProvider<CardsRepository>.value(value: cardsRepo),
        RepositoryProvider<WalletRepository>.value(value: walletRepo),
      ],
      child: MultiBlocProvider(
        providers: [
          BlocProvider<ThemeCubit>(create: (_) => ThemeCubit()),
          BlocProvider<AuthBloc>(create: (_) => AuthBloc(authRepo)..add(AppStarted())),
          BlocProvider<ItemsBloc>(create: (_) => ItemsBloc(itemRepo)..add(ItemsRequested())),
        ],
        child: Builder(
          builder: (context) {
            final router = AppRouter(authBloc: context.read<AuthBloc>()).router;
            return BlocBuilder<ThemeCubit, ThemeMode>(
              builder: (_, mode) => MaterialApp.router(
                debugShowCheckedModeBanner: false,
                title: 'Flutter Web BLoC Starter',
                theme: AppTheme.light,
                darkTheme: AppTheme.dark,
                themeMode: mode,
                routerConfig: router,
              ),
            );
          },
        ),
      ),
    );
  }
}
