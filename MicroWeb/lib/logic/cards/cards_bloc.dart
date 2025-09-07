import 'dart:async';

import 'package:equatable/equatable.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:micro_web/data/models/requests/assign_card_request.dart';
import 'package:micro_web/data/models/requests/create_card_request.dart';

import '../../data/models/responses/card_response.dart';
import '../../data/models/responses/user_response.dart';
import '../../data/repositories/cards_repository.dart';
import '../../data/repositories/users_repository.dart';

part 'cards_event.dart';
part 'cards_state.dart';

class CardsBloc extends Bloc<CardsEvent, CardsState> {
  CardsBloc(this._repo, this._usersRepo) : super(const CardsState.initial()) {
    on<CardsRequested>(_onRequested);
    on<CardCreateRequested>(_onCreate);
    on<UsersLoadRequested>(_onUsersLoad);
    on<CardAssignRequested>(_onAssign);
    on<CardTerminationRequested>(_onTerminate);
  }

  final CardsRepository _repo;
  final UsersRepository _usersRepo;

  Future<void> _onRequested(CardsRequested event, Emitter<CardsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final data = await _repo.listCards();
      final list = CardResponse.listFromJson(data);
      emit(state.copyWith(loading: false, items: list, error: null));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }

  Future<void> _onCreate(CardCreateRequested event, Emitter<CardsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final data = await _repo.createCard(event.request);
      final card = CardResponse.fromJson(data);
      final updatedItems = List<CardResponse>.from(state.items)..add(card);
      emit(state.copyWith(loading: false, items: updatedItems, error: null));
      event.completer.complete(card);
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
      event.completer.completeError(e);
    }
  }

  Future<void> _onTerminate(CardTerminationRequested event, Emitter<CardsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      await _repo.terminateCard(event.id);
      emit(state.copyWith(loading: false, error: null));
      event.completer.complete();
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
      event.completer.completeError(e);
    }
  }

  Future<void> _onAssign(CardAssignRequested event, Emitter<CardsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      await _repo.assignCard(event.id, AssignCardRequest(userId: event.userId));
      emit(state.copyWith(loading: false, error: null));
      event.completer.complete();
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
      event.completer.completeError(e);
    }
  }

  Future<CardResponse?> create(CreateCardRequest request) {
    final completer = Completer<CardResponse?>();
    add(CardCreateRequested(request, completer));
    return completer.future;
  }

  Future<void> _onUsersLoad(UsersLoadRequested event, Emitter<CardsState> emit) async {
    try {
      final users = await _usersRepo.listUsers();
      emit(state.copyWith(users: users));
    } catch (e) {
      if (kDebugMode) {
        print('‼️CardsBloc: loadUsers error: $e');
      }
    }
  }

  Future<void> assignUser(String cardId, String selectedUserId) {
    final completer = Completer<void>();
    add(CardAssignRequested(cardId, selectedUserId, completer));
    return completer.future;
  }
}
