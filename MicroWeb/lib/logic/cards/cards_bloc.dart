import 'dart:async';

import 'package:equatable/equatable.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:micro_web/data/models/requests/assign_card_request.dart';
import 'package:micro_web/data/models/requests/create_card_request.dart';
import 'package:micro_web/data/models/requests/update_card_request.dart';

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
    on<CardUpdateRequested>(_onUpdate);
    on<CardPrintRequested>(_onPrint);
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
      // Optimistically mark the card as terminated in the local state so UI updates immediately
      final updatedItems = [...state.items];
      final idx = updatedItems.indexWhere((e) => e.id == event.id);
      if (idx != -1) {
        final c = updatedItems[idx];
        updatedItems[idx] = CardResponse(
          id: c.id,
          type: c.type,
          name: c.name,
          singleTransactionLimit: c.singleTransactionLimit,
          monthlyLimit: c.monthlyLimit,
          assignedUserId: c.assignedUserId,
          options: c.options,
          printed: c.printed,
          terminated: true,
          createdAt: c.createdAt,
          updatedAt: DateTime.now(),
        );
      }
      emit(state.copyWith(loading: false, items: updatedItems, error: null));
      event.completer.complete();
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
      event.completer.completeError(e);
    }
  }

  Future<void> _onPrint(CardPrintRequested event, Emitter<CardsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      await _repo.updateCard(event.id, UpdateCardRequest(printed: true));
      // Optimistically mark the card as printed in the local state so UI updates immediately
      final updatedItems = [...state.items];
      final idx = updatedItems.indexWhere((e) => e.id == event.id);
      if (idx != -1) {
        final c = updatedItems[idx];
        updatedItems[idx] = CardResponse(
          id: c.id,
          type: c.type,
          name: c.name,
          singleTransactionLimit: c.singleTransactionLimit,
          monthlyLimit: c.monthlyLimit,
          assignedUserId: c.assignedUserId,
          options: c.options,
          printed: true,
          terminated: c.terminated,
          createdAt: c.createdAt,
          updatedAt: DateTime.now(),
        );
      }
      emit(state.copyWith(loading: false, items: updatedItems, error: null));
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
      // Optimistically set assignedUserId in local state so UI updates immediately
      final updatedItems = [...state.items];
      final idx = updatedItems.indexWhere((e) => e.id == event.id);
      if (idx != -1) {
        final c = updatedItems[idx];
        updatedItems[idx] = CardResponse(
          id: c.id,
          type: c.type,
          name: c.name,
          singleTransactionLimit: c.singleTransactionLimit,
          monthlyLimit: c.monthlyLimit,
          assignedUserId: event.userId,
          options: c.options,
          printed: c.printed,
          terminated: c.terminated,
          createdAt: c.createdAt,
          updatedAt: DateTime.now(),
        );
      }
      emit(state.copyWith(loading: false, items: updatedItems, error: null));
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

  Future<CardResponse?> update(String id, UpdateCardRequest request) {
    final completer = Completer<CardResponse?>();
    add(CardUpdateRequested(id, request, completer));
    return completer.future;
  }

  Future<void> terminate(String id) async {
    final completer = Completer<void>();
    add(CardTerminationRequested(id, completer));
    return completer.future;
  }

  Future<void> printCard(String id) async {
    final completer = Completer<void>();
    add(CardPrintRequested(id, completer));
    return completer.future;
  }

  Future<void> _onUpdate(CardUpdateRequested event, Emitter<CardsState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final data = await _repo.updateCard(event.id, event.request);
      final card = CardResponse.fromJson(Map<String, dynamic>.from(data["card"] as Map));
      final updatedItems = [...state.items];
      final idx = updatedItems.indexWhere((e) => e.id == card.id);
      if (idx == -1) {
        updatedItems.add(card);
      } else {
        updatedItems[idx] = card;
      }
      emit(state.copyWith(loading: false, items: updatedItems, error: null));
      event.completer.complete(card);
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
      event.completer.completeError(e);
    }
  }
}
