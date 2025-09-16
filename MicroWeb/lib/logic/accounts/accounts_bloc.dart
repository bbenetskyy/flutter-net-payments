import 'dart:async';

import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../data/models/requests/create_account_request.dart';
import '../../data/models/responses/account_response.dart';
import '../../data/repositories/accounts_repository.dart';

part 'accounts_event.dart';
part 'accounts_state.dart';

class AccountsBloc extends Bloc<AccountsEvent, AccountsState> {
  AccountsBloc(this._repo) : super(const AccountsState.initial()) {
    on<AccountsRequested>(_onRequested);
    on<AccountCreateRequested>(_onCreate);
    on<AccountDeleteRequested>(_onDelete);
  }

  final AccountsRepository _repo;

  Future<void> _onRequested(AccountsRequested event, Emitter<AccountsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final list = await _repo.listMyAccounts();
      emit(state.copyWith(loading: false, items: list));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }

  Future<void> _onCreate(AccountCreateRequested event, Emitter<AccountsState> emit) async {
    emit(state.copyWith(submitting: true, submitError: null));
    try {
      final created = await _repo.createAccount(event.request);
      final updated = [created, ...state.items];
      emit(state.copyWith(submitting: false, items: updated));
      event.completer.complete(created);
    } catch (e) {
      emit(state.copyWith(submitting: false, submitError: e.toString()));
      event.completer.completeError(e);
    }
  }

  Future<void> _onDelete(AccountDeleteRequested event, Emitter<AccountsState> emit) async {
    emit(state.copyWith(submitting: true, submitError: null));
    try {
      await _repo.deleteAccount(event.id);
      // Optimistically remove from list
      final updated = state.items.where((e) => e.id != event.id).toList();
      emit(state.copyWith(submitting: false, items: updated));
      event.completer.complete();
    } catch (e) {
      emit(state.copyWith(submitting: false, submitError: e.toString()));
      event.completer.completeError(e);
    }
  }

  // Convenience API for UI
  void load() => add(const AccountsRequested());
  Future<AccountResponse?> create(CreateAccountRequest request) {
    final c = Completer<AccountResponse?>();
    add(AccountCreateRequested(request, c));
    return c.future;
  }

  Future<void> delete(String id) {
    final c = Completer<void>();
    add(AccountDeleteRequested(id, c));
    return c.future;
  }
}
