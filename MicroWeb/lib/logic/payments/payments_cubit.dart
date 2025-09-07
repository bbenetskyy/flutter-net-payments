import 'package:equatable/equatable.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../data/models/requests/create_payment_request.dart';
import '../../data/models/responses/payment_response.dart';
import '../../data/models/responses/user_response.dart';
import '../../data/models/responses/account_response.dart';
import '../../data/repositories/payments_repository.dart';
import '../../data/repositories/users_repository.dart';
import '../../data/repositories/accounts_repository.dart';

class PaymentsState extends Equatable {
  const PaymentsState({
    this.loading = false,
    this.items = const [],
    this.error,
    this.submitting = false,
    this.submitError,
    this.currentUser,
    this.myAccounts = const [],
    this.users = const [],
    this.beneficiaryAccounts = const [],
  });

  final bool loading;
  final List<PaymentResponse> items;
  final String? error;

  final bool submitting;
  final String? submitError;

  final UserResponse? currentUser;
  final List<AccountResponse> myAccounts;
  final List<UserResponse> users;
  final List<AccountResponse> beneficiaryAccounts;

  PaymentsState copyWith({
    bool? loading,
    List<PaymentResponse>? items,
    String? error,
    bool? submitting,
    String? submitError,
    UserResponse? currentUser,
    List<AccountResponse>? myAccounts,
    List<UserResponse>? users,
    List<AccountResponse>? beneficiaryAccounts,
  }) {
    return PaymentsState(
      loading: loading ?? this.loading,
      items: items ?? this.items,
      error: error,
      submitting: submitting ?? this.submitting,
      submitError: submitError,
      currentUser: currentUser ?? this.currentUser,
      myAccounts: myAccounts ?? this.myAccounts,
      users: users ?? this.users,
      beneficiaryAccounts: beneficiaryAccounts ?? this.beneficiaryAccounts,
    );
  }

  @override
  List<Object?> get props => [
    loading,
    items,
    error,
    submitting,
    submitError,
    currentUser,
    myAccounts,
    users,
    beneficiaryAccounts,
  ];
}

class PaymentsCubit extends Cubit<PaymentsState> {
  PaymentsCubit(this._repo, this._usersRepo, this._accountsRepo) : super(const PaymentsState());

  final PaymentsRepository _repo;
  final UsersRepository _usersRepo;
  final AccountsRepository _accountsRepo;

  Future<void> prefetchFormLookups() async {
    try {
      // Load users list for dropdown; also keep first as current user for header.
      final users = await _usersRepo.listUsers();
      final me = await _usersRepo.getMe();

      //remove me from users
      users.removeWhere((u) => u.id == me.id);

      // Load my accounts for IBAN dropdown
      final accounts = await _accountsRepo.listMyAccounts();

      emit(state.copyWith(currentUser: me, users: users, myAccounts: accounts));
    } catch (e) {
      if (kDebugMode) {
        print('‼️PaymentsCubit: prefetchFormLookups error: $e');
      }
    }
  }

  Future<void> loadBeneficiaryAccounts(String? userId) async {
    if (userId == null || userId.isEmpty) {
      emit(state.copyWith(beneficiaryAccounts: const []));
      return;
    }
    try {
      final list = await _accountsRepo.listUserAccounts(userId);
      emit(state.copyWith(beneficiaryAccounts: list));
    } catch (e) {
      if (kDebugMode) {
        print('‼️PaymentsCubit: loadBeneficiaryAccounts error: $e');
      }
      emit(state.copyWith(beneficiaryAccounts: const []));
    }
  }

  Future<void> load({Map<String, dynamic>? query}) async {
    emit(state.copyWith(loading: true, error: null));
    await prefetchFormLookups();
    try {
      final data = await _repo.listPayments(query: query);
      final list = PaymentResponse.listFromJson(data);
      emit(state.copyWith(loading: false, items: list, error: null));
    } catch (e) {
      //ignore
      // emit(state.copyWith(loading: false, error: e.toString()));
    }
  }

  Future<PaymentResponse?> create(CreatePaymentRequest request) async {
    emit(state.copyWith(submitting: true, submitError: null));
    try {
      final data = await _repo.createPayment(request);
      final created = PaymentResponse.fromJson(data['payment']);
      final updated = [created, ...state.items];
      emit(state.copyWith(submitting: false, items: updated));
      return created;
    } catch (e) {
      emit(state.copyWith(submitting: false, submitError: e.toString()));
      return null;
    }
  }
}
