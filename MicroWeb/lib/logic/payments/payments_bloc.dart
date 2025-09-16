import 'dart:async';

import 'package:equatable/equatable.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:micro_web/logic/payments/payments_cubit.dart';
import 'package:micro_web/logic/payments/payments_event.dart';

import '../../data/models/requests/create_payment_request.dart';
import '../../data/models/responses/account_response.dart';
import '../../data/models/responses/payment_response.dart';
import '../../data/models/responses/user_response.dart';
import '../../data/repositories/accounts_repository.dart';
import '../../data/repositories/payments_repository.dart';
import '../../data/repositories/users_repository.dart';
import '../../data/models/requests/create_verification_request.dart';
import '../../data/models/verification_action.dart';

class PaymentsBloc extends Bloc<PaymentsEvent, PaymentsState> {
  PaymentsBloc(this._repo, this._usersRepo, this._accountsRepo) : super(const PaymentsState()) {
    on<PaymentsRequested>(_onPaymentsRequested);
    on<PaymentsPrefetchRequested>(_onPrefetch);
    on<BeneficiaryAccountsRequested>(_onBeneficiaryAccounts);
    on<PaymentCreateRequested>(_onCreate);
    on<PaymentRevertRequested>(_onRevert);
  }

  final PaymentsRepository _repo;
  final UsersRepository _usersRepo;
  final AccountsRepository _accountsRepo;

  // Convenience methods to keep UI changes minimal
  Future<void> load({Map<String, dynamic>? query}) async => add(PaymentsRequested(query: query));
  Future<void> prefetchFormLookups() async => add(const PaymentsPrefetchRequested());
  Future<void> loadBeneficiaryAccounts(String? userId) async => add(BeneficiaryAccountsRequested(userId));
  Future<PaymentResponse?> create(CreatePaymentRequest request) {
    final completer = Completer<PaymentResponse?>();
    add(PaymentCreateRequested(request, completer));
    return completer.future;
  }

  Future<void> revert(String paymentId) {
    final completer = Completer<void>();
    add(PaymentRevertRequested(paymentId, completer));
    return completer.future;
  }

  Future<void> _onPrefetch(PaymentsPrefetchRequested event, Emitter<PaymentsState> emit) async {
    try {
      final users = await _usersRepo.listUsers();
      final me = await _usersRepo.getMe();
      users.removeWhere((u) => u.id == me.id);
      final accounts = await _accountsRepo.listMyAccounts();
      emit(state.copyWith(currentUser: me, users: users, myAccounts: accounts));
    } catch (e) {
      if (kDebugMode) {
        print('‼️PaymentsBloc: prefetchFormLookups error: $e');
      }
    }
  }

  Future<void> _onBeneficiaryAccounts(BeneficiaryAccountsRequested event, Emitter<PaymentsState> emit) async {
    final userId = event.userId;
    if (userId == null || userId.isEmpty) {
      emit(state.copyWith(beneficiaryAccounts: const []));
      return;
    }
    try {
      final list = await _accountsRepo.listUserAccounts(userId);
      emit(state.copyWith(beneficiaryAccounts: list));
    } catch (e) {
      if (kDebugMode) {
        print('‼️PaymentsBloc: loadBeneficiaryAccounts error: $e');
      }
      emit(state.copyWith(beneficiaryAccounts: const []));
    }
  }

  Future<void> _onPaymentsRequested(PaymentsRequested event, Emitter<PaymentsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    await _onPrefetch(const PaymentsPrefetchRequested(), emit);
    try {
      final data = await _repo.listPayments(query: event.query);
      final list = PaymentResponse.listFromJson(data);
      emit(state.copyWith(loading: false, items: list, error: null));
    } catch (e) {
      // Keeping previous behavior to ignore errors silently in load
      emit(state.copyWith(loading: false));
    }
  }

  Future<void> _onCreate(PaymentCreateRequested event, Emitter<PaymentsState> emit) async {
    emit(state.copyWith(submitting: true, submitError: null));
    await _onPrefetch(const PaymentsPrefetchRequested(), emit);
    try {
      final data = await _repo.createPayment(event.request);
      final created = PaymentResponse.fromJson(Map<String, dynamic>.from(data['payment'] as Map));
      final updated = [created, ...state.items];
      emit(state.copyWith(submitting: false, items: updated));
      event.completer.complete(created);
    } catch (e) {
      emit(state.copyWith(submitting: false, submitError: e.toString()));
      event.completer.complete(null);
    }
  }

  Future<void> _onRevert(PaymentRevertRequested event, Emitter<PaymentsState> emit) async {
    emit(state.copyWith(submitting: true, submitError: null));
    try {
      final req = CreateVerificationRequest(action: VerificationAction.PaymentReverted, targetId: event.paymentId);
      await _repo.createPaymentVerification(req);
      // Optimistically update payment status so list and details reflect immediately
      final updatedItems = [...state.items];
      final idx = updatedItems.indexWhere((e) => e.id == event.paymentId);
      if (idx != -1) {
        final p = updatedItems[idx];
        updatedItems[idx] = PaymentResponse(
          id: p.id,
          userId: p.userId,
          beneficiaryName: p.beneficiaryName,
          beneficiaryAccount: p.beneficiaryAccount,
          beneficiaryId: p.beneficiaryId,
          beneficiaryAccountId: p.beneficiaryAccountId,
          fromAccount: p.fromAccount,
          amount: p.amount,
          currency: p.currency,
          fromCurrency: p.fromCurrency,
          details: p.details,
          status: 'AwaitingReversion',
          createdAt: p.createdAt,
          updatedAt: DateTime.now(),
        );
      }
      emit(state.copyWith(submitting: false, items: updatedItems));
      event.completer.complete();
    } catch (e) {
      emit(state.copyWith(submitting: false, submitError: e.toString()));
      event.completer.completeError(e);
    }
  }
}
