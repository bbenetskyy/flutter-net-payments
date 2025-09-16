import 'package:equatable/equatable.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:micro_web/logic/users/users_cubit.dart';
import 'package:micro_web/logic/users/users_event.dart';

import '../../data/models/requests/update_user_request.dart';
import '../../data/models/responses/user_response.dart';
import '../../data/repositories/users_repository.dart';

class UsersBloc extends Bloc<UsersEvent, UsersState> {
  UsersBloc(this._repo) : super(const UsersState()) {
    on<UsersRequested>(_onRequested);
    on<UsersUpdateRequested>(_onUpdateRequested);
  }

  final UsersRepository _repo;

  Future<void> load() async => add(const UsersRequested());

  Future<void> _onRequested(UsersRequested event, Emitter<UsersState> emit) async {
    emit(state.copyWith(loading: true, error: null));
    try {
      final me = await _repo.getMe();
      final list = await _repo.listUsers();
      final roles = await _repo.listRoles();
      list.removeWhere((u) => (u.id != null && u.id == me.id));
      emit(state.copyWith(loading: false, users: list, error: null, roles: roles));
    } catch (e) {
      emit(state.copyWith(loading: false, error: e.toString()));
    }
  }

  Future<void> _onUpdateRequested(UsersUpdateRequested event, Emitter<UsersState> emit) async {
    try {
      // Build request in BLoC as required
      final req = UpdateUserRequest(
        displayName: (event.displayName?.trim().isEmpty ?? true) ? null : event.displayName!.trim(),
        roleId: event.roleId,
        dateOfBirth: event.dateOfBirth,
      );
      final updatedUser = await _repo.updateUser(event.userId, req);
      final users = await _repo.listUsers();
      emit(state.copyWith(error: null, loading: false, users: users));
      event.completer?.complete(updatedUser);
    } catch (e) {
      event.completer?.completeError(e);
      emit(state.copyWith(error: e.toString()));
    }
  }
}
