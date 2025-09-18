import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../data/models/responses/ledger_response.dart';
import '../../data/models/responses/wallet_response.dart';
import '../../data/models/responses/user_response.dart';
import '../../data/models/currency.dart';
import '../../data/models/top_up_request.dart';
import '../../data/repositories/users_repository.dart';
import '../../data/repositories/wallet_repository.dart';
import 'package:uuid/uuid.dart';

class WalletState extends Equatable {
  const WalletState({this.loading = false, this.error, this.wallet, this.ledger = const [], this.currentUser});

  final bool loading;
  final String? error;
  final WalletResponse? wallet;
  final List<LedgerResponse> ledger;
  final UserResponse? currentUser;

  WalletState copyWith({
    bool? loading,
    String? error,
    WalletResponse? wallet,
    List<LedgerResponse>? ledger,
    UserResponse? currentUser,
  }) {
    return WalletState(
      loading: loading ?? this.loading,
      error: error,
      wallet: wallet ?? this.wallet,
      ledger: ledger ?? this.ledger,
      currentUser: currentUser ?? this.currentUser,
    );
  }

  @override
  List<Object?> get props => [loading, error, wallet, ledger, currentUser];
}

class WalletCubit extends Cubit<WalletState> {
  WalletCubit(this._walletRepo, this._usersRepo) : super(const WalletState());

  final WalletRepository _walletRepo;
  final UsersRepository _usersRepo;

  Future<void> loadForCurrentUser() async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final me = await _usersRepo.getMe();
      final wallet = await _walletRepo.getWalletByUserId(me.id ?? '');
      final ledger = await _walletRepo.getLedgerByUserId(me.id ?? '');
      if (ledger.isNotEmpty) {
        ledger.sort((a, b) => b.createdAt?.compareTo(a.createdAt ?? DateTime.now()) ?? 0);
      }
      emit(state.copyWith(loading: false, wallet: wallet, ledger: ledger, currentUser: me));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }

  Future<void> topUp({required int amountMajor, required Currency currency, String? description}) async {
    // amountMinor: converting assuming 2 decimal places (e.g., cents)
    final amountMinor = amountMajor * 100;
    final userId = state.currentUser?.id;
    if (userId == null || userId.isEmpty) return;
    emit(state.copyWith(loading: true, error: null));
    try {
      final req = TopUpRequest(
        amountMinor: amountMinor,
        currency: currency,
        correlationId: const Uuid().v4(),
        description: description ?? 'Top up',
      );
      await _walletRepo.topUp(userId, req);
      // Reload wallet and ledger after top-up
      final wallet = await _walletRepo.getWalletByUserId(userId);
      final ledger = await _walletRepo.getLedgerByUserId(userId);
      if (ledger.isNotEmpty) {
        ledger.sort((a, b) => b.createdAt?.compareTo(a.createdAt ?? DateTime.now()) ?? 0);
      }
      emit(state.copyWith(loading: false, wallet: wallet, ledger: ledger));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }
}
